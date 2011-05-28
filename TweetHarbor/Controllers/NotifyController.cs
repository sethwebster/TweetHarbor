using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using TweetHarbor.Models;
using TweetHarbor.Data;

namespace TweetHarbor.Controllers
{
    public class NotifyController : Controller
    {
        ITweetHarborDbContext database;

        public NotifyController(ITweetHarborDbContext database)
        {
            this.database = database;
        }

        public ActionResult New()
        {
            return Redirect("/?f=newnotify");
        }

        [HttpPost]
        public JsonResult New(string Id, string token, Notification notification)
        {
            var u = database.Users.Include("Projects").FirstOrDefault(usr => usr.TwitterUserName == Id && usr.UniqueId == token);
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
                        SendPrivateTweetOnFailure = true,
                        SendPrivateTweetOnSuccess = true,
                        SendPublicTweetOnFailure = false,
                        SendPublicTweetOnSuccess = true,
                        User = u
                    };
                    u.Projects.Add(project);
                    //TODO: Add logging to prevent these errors from being swallowed
                    database.SaveChanges();
                }
                if (null != project)
                {
                    TweetSharp.TwitterService s = new TweetSharp.TwitterService(TwitterHelper.ConsumerKey, TwitterHelper.ConsumerSecret);
                    s.AuthenticateWith(u.OAuthToken, u.OAuthTokenSecret);
                    //TODO: Update for templates
                    var stringUpdate = string.Format("Application {0} build {1}, {2}", notification.application.name, notification.build.status, notification.build.commit.message);
                    if (stringUpdate.Length > 140)
                        stringUpdate = stringUpdate.Substring(0, 136) + "...";

                    if (notification.build.status == "succeeded")
                    {
                        if (project.SendPrivateTweetOnSuccess && u.SendPrivateTweet)
                        {
                            var dmRes = s.SendDirectMessage(u.TwitterUserName, stringUpdate);
                        }
                        if (project.SendPublicTweetOnSuccess && u.SendPublicTweet)
                        {
                            var pubRes = s.SendTweet(stringUpdate);
                        }
                    }
                    else
                    {
                        if (project.SendPrivateTweetOnFailure && u.SendPrivateTweet)
                        {
                            var dmRes = s.SendDirectMessage(u.TwitterUserName, stringUpdate);
                        }
                        if (project.SendPublicTweetOnFailure && u.SendPublicTweet)
                        {
                            var pubRes = s.SendTweet(stringUpdate);
                        }
                    }
                    return Json(new { Success = true });
                }
                else
                {
                    return Json(new { Success = false, Error = "Unable to locate or create project" });
                }
            }
            else
            {
                return Json(new { Success = false, Error = "NotAuthorized" });
            }

        }





        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }
    }
}
