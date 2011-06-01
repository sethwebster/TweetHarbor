using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using TweetHarbor.Models;
using TweetHarbor.Data;
using TweetSharp;
using TweetHarbor.Messaging;

namespace TweetHarbor.Controllers
{
    public class NotifyController : Controller
    {
        ITweetHarborDbContext database;
        ITweetHarborTwitterService twitter;
        public NotifyController(ITweetHarborDbContext database, ITweetHarborTwitterService twitter)
        {
            this.database = database;
            this.twitter = twitter;
            
        }

        public ActionResult New()
        {
            return Redirect("/?f=newnotify");
        }

        [HttpPost]
        public JsonResult New(string Id, string token, Notification notification)
        {
            var u = database.Users
                .Include("Projects")
                .Include("Projects.MessageRecipients")
                .FirstOrDefault(usr => usr.TwitterUserName == Id && usr.UniqueId == token);
            if (null != u)
            {
                var project = u.Projects.FirstOrDefault(p => p.ProjectName == notification.application.name);
                if (null == project)
                {
                    project = new Project() 
                    {
                        ProjectName = notification.application.name,
                        FailureTemplate = "",
                        SuccessTemplate = "",                       
                        User = u
                    };
                    u.Projects.Add(project);
                    //TODO: Add logging to prevent these errors from being swallowed
                    database.SaveChanges();
                }
                if (null != project)
                {
                    twitter.AuthenticateWith(u.OAuthToken, u.OAuthTokenSecret);

                    if (notification.build.status == "succeeded")
                    {
                        var strSuccessUpdate = string.IsNullOrEmpty(project.SuccessTemplate) ? Properties.Settings.Default.DefaultSuccessTemplate : project.SuccessTemplate;
                        strSuccessUpdate = strSuccessUpdate.Replace("{application:name}", project.ProjectName)
                            .Replace("{build:commit:message}", notification.build.commit.message)
                            .Replace("{build:commit:id}", notification.build.commit.id);
                        if (strSuccessUpdate.Length > 140)
                            strSuccessUpdate = strSuccessUpdate.Substring(0, 136) + "...";

                        if (project.SendPrivateTweetOnSuccess && u.SendPrivateTweet)
                        {
                           // TwitterDirectMessage dmRes = twitter.SendDirectMessage(u.TwitterUserName, strSuccessUpdate);
                            SendDirectMessages(project, strSuccessUpdate);
                        }
                        if (project.SendPublicTweetOnSuccess && u.SendPublicTweet)
                        {
                            TwitterStatus pubRes = twitter.SendTweet(strSuccessUpdate);
                        }
                    }
                    else
                    {
                        var strFailureUpdate = string.IsNullOrEmpty(project.FailureTemplate) ? Properties.Settings.Default.DefaultFailureTemplate : project.FailureTemplate;
                        strFailureUpdate = strFailureUpdate.Replace("{application:name}", project.ProjectName).Replace("{build:commit:message}", notification.build.commit.message);
                        if (strFailureUpdate.Length > 140)
                            strFailureUpdate = strFailureUpdate.Substring(0, 136) + "...";

                        if (project.SendPrivateTweetOnFailure && u.SendPrivateTweet)
                        {
                          //  var dmRes = twitter.SendDirectMessage(u.TwitterUserName, strFailureUpdate);
                            SendDirectMessages(project, strFailureUpdate);
                        }
                        if (project.SendPublicTweetOnFailure && u.SendPublicTweet)
                        {
                            var pubRes = twitter.SendTweet(strFailureUpdate);
                        }
                    }
                    return Json(new JsonResultModel() { Success = true });
                }
                else
                {
                    return Json(new JsonResultModel() { Success = false, Error = "Unable to locate or create project" });
                }
            }
            else
            {
                return Json(new JsonResultModel() { Success = false, Error = "NotAuthorized" });
            }

        }

        private void SendDirectMessages(Project project, string update)
        {
            foreach (var r in project.MessageRecipients)
            {
                twitter.SendDirectMessage(r.ScreenName, update);
            }
        }





        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }
    }
}
