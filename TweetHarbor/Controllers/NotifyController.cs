﻿using System;
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
        ITweetHarborTextMessageService textMessageService;

        public NotifyController(ITweetHarborDbContext database, ITweetHarborTwitterService twitter, ITweetHarborTextMessageService textMessageService)
        {
            this.database = database;
            this.twitter = twitter;
            this.textMessageService = textMessageService;
        }

        public ActionResult New()
        {
            return Redirect("/?f=newnotify");
        }

        [HttpPost]
        public JsonResult New(string Id, string token, Notification notification)
        {
            // Get the User based on supplied Id
            // * Token must match *
            var user = database.Users
                .Include("Projects")
                .Include("Projects.TwitterAccounts")
                .Include("Projects.MessageRecipients")
                .Include("Projects.TextMessageRecipients")
                .FirstOrDefault(usr => usr.UserName == Id && usr.UniqueId == token);
            // If Id or Token is invalid, user will not be found
            // TODO: Allow users to reset the token
            if (null != user)
            {
                // Locate or create our project
                var project = CreateProjectIfNecessary(notification, user);
                if (null != project)
                {
                    SaveNotification(notification, project);

                    // If the message ends with a dash, we are NOT notifying anyone of the push
                    if (notification.build.commit.message.Trim().EndsWith("-") == false)
                    {
                        // start our connection to twitter
                        var twitterAccount = project.TwitterAccounts != null ? project.TwitterAccounts.FirstOrDefault() : null;
                        if (null == twitterAccount)
                            twitterAccount = user.AuthenticationAccounts.FirstOrDefault(a => a.AccountProvider.ToLower() == "twitter");
                        //TODO: Ensure we HAVE a twitter account
                        twitter.AuthenticateWith(twitterAccount.OAuthToken, twitterAccount.OAuthTokenSecret);

                        // Format and send appropriate messages
                        if (notification.build.status == "succeeded")
                        {
                            // Get the Success Template (or default)
                            var strSuccessUpdate = string.IsNullOrEmpty(project.SuccessTemplate) ?
                                Properties.Settings.Default.DefaultSuccessTemplate : project.SuccessTemplate;

                            // Replace tokens
                            strSuccessUpdate = DeTokenizeString(strSuccessUpdate, project, notification);

                            if (strSuccessUpdate.Length > 140)
                                strSuccessUpdate = strSuccessUpdate.Substring(0, 136) + "...";

                            // ensure we're 'authorized' to send the tweet
                            if (project.SendPrivateTweetOnSuccess && user.SendPrivateTweet)
                            {
                                SendDirectMessages(project, strSuccessUpdate);
                            }
                            if (project.SendPublicTweetOnSuccess && user.SendPublicTweet)
                            {
                                TwitterStatus pubRes = twitter.SendTweet(strSuccessUpdate);
                            }
                            if (project.SendTextOnSuccess && user.SendSMS)
                            {
                                SendSmsMessages(project, strSuccessUpdate);
                            }
                        }
                        else
                        {
                            // Get the Failure Template (or default)
                            var strFailureUpdate = string.IsNullOrEmpty(project.FailureTemplate) ?
                                Properties.Settings.Default.DefaultFailureTemplate : project.FailureTemplate;

                            // Replace tokens & clean up 
                            strFailureUpdate = DeTokenizeString(strFailureUpdate, project, notification);

                            if (strFailureUpdate.Length > 140)
                                strFailureUpdate = strFailureUpdate.Substring(0, 136) + "...";

                            // ensure we're 'authorized' to send the tweet
                            if (project.SendPrivateTweetOnFailure && user.SendPrivateTweet)
                            {
                                SendDirectMessages(project, strFailureUpdate);
                            }
                            if (project.SendPublicTweetOnFailure && user.SendPublicTweet)
                            {
                                var pubRes = twitter.SendTweet(strFailureUpdate);
                            }
                            if (project.SendTextOnFailure && user.SendSMS)
                            {
                                SendSmsMessages(project, strFailureUpdate);
                            }
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

        private void SendSmsMessages(Project project, string update)
        {
            if (null != project.TextMessageRecipients && project.TextMessageRecipients.Count > 0)
            {
                foreach (var r in project.TextMessageRecipients)
                {
                    try
                    {
                        OutboundNotification n = new OutboundNotification()
                        {
                            DateCreated = DateTime.Now,
                            Message = update,
                            NotificationType = "SMS",
                            Recipient = r.PhoneNumber,
                            SentSuccessfully = false,
                            DateSent = DateTime.MaxValue
                        };
                        project.OutboundNotifications.Add(n);
                        database.SaveChanges();
                        try
                        {
                            textMessageService.SendText(r.PhoneNumber, update);
                            n.SentSuccessfully = true;
                            n.DateSent = DateTime.Now;
                        }
                        catch (Exception sx)
                        {
                            n.SentSuccessfully = false;
                        }
                        database.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        //TODO: Log this
                        throw e;
                    }
                }
            }
        }

        private void SaveNotification(Notification notification, Project project)
        {
            ProjectNotification projNotification = new ProjectNotification();
            projNotification.NotificationDate = DateTime.Now;
            projNotification.Build = notification.build;
            project.ProjectNotifications.Add(projNotification);
            database.SaveChanges();
        }

        // TODO: Create a richer tokenization approach (string replace isn't pretty, but worked)
        [NonAction]
        public static string DeTokenizeString(string input, Project project, Notification notification)
        {

            return input.Replace("{application:name}", project.ProjectName)
                            .Replace("{build:commit:message}", notification.build.commit.message)
                            .Replace("{build:commit:id}", notification.build.commit.id.Substring(0, 7));
        }

        [NonAction]
        private Project CreateProjectIfNecessary(Notification notification, Models.User user)
        {
            var project = user.Projects.FirstOrDefault(p => p.ProjectName == notification.application.name);
            if (null == project)
            {
                project = new Project()
                {
                    ProjectName = notification.application.name,
                    FailureTemplate = "",
                    SuccessTemplate = "",
                    User = user
                };
                user.Projects.Add(project);
                //TODO: Add logging to prevent these errors from being swallowed
                database.SaveChanges();
            }
            return project;
        }

        [NonAction]
        private void SendDirectMessages(Project project, string update)
        {
            foreach (var r in project.MessageRecipients)
            {
                try
                {
                    OutboundNotification n = new OutboundNotification()
                    {
                        DateCreated = DateTime.Now,
                        Message = update,
                        NotificationType = "Twitter",
                        Recipient = r.ScreenName,
                        SentSuccessfully = false,
                        DateSent = DateTime.MaxValue
                    };
                    project.OutboundNotifications.Add(n);
                    database.SaveChanges();
                    try
                    {
                        twitter.SendDirectMessage(r.ScreenName, update);
                        n.SentSuccessfully = true;
                        n.DateSent = DateTime.Now;
                    }
                    catch (Exception sx)
                    {
                        n.SentSuccessfully = false;
                    }
                    database.SaveChanges();
                }
                catch (Exception e)
                {
                    //TODO: Log this
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }
    }
}
