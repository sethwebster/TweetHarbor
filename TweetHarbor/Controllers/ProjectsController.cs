﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;
using TweetHarbor.Models;
using Newtonsoft.Json;
using System.Data.Entity.Validation;

namespace TweetHarbor.Controllers
{
    public class ProjectsController : Controller
    {

        ITweetHarborDbContext database = null;

        public ProjectsController(ITweetHarborDbContext database)
        {
            this.database = database;
        }
        //
        // GET: /Projects/

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public JsonResult UserProjects()
        {
            if (null != HttpContext)
            {
                var user = database.Users.Include("Projects")
                 .Include("Projects.MessageRecipients")
                 .Include("Projects.TextMessageRecipients")
                 .Include("Projects.ProjectNotifications")
                 .Include("Projects.ProjectNotifications.Build")
                 .Include("Projects.ProjectNotifications.Build.commit")
                 .FirstOrDefault(usr => usr.TwitterUserName == HttpContext.User.Identity.Name);
                foreach (var p in user.Projects)
                {
                    foreach (var m in p.MessageRecipients)
                    {
                        m.Projects = null;
                    }
                    foreach (var m in p.ProjectNotifications)
                    {
                        m.Project = null;
                    }
                    foreach (var m in p.TextMessageRecipients)
                    {
                        m.Projects = null;
                    }
                    if (null != p.User)
                    {
                        foreach (var j in p.User.Projects)
                        {
                            j.User = null;
                        }
                    }

                }
                return Json(user.Projects, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Error = "Not and Http Request" });
        }

        [HttpPost]
        [Authorize]
        public JsonResult ProjectNotificationToggle(string Id, string TweetType, bool Value)
        {
            if (null != HttpContext)
            {
                var prj = database.Projects.FirstOrDefault(p => p.User.TwitterUserName == HttpContext.User.Identity.Name && p.ProjectName == Id);

                if (null != prj)
                {
                    switch (TweetType)
                    {
                        case "SendPublicTweetOnSuccess":
                            prj.SendPublicTweetOnSuccess = Value;
                            break;
                        case "SendPrivateTweetOnSuccess":
                            prj.SendPrivateTweetOnSuccess = Value;
                            break;
                        case "SendPublicTweetOnFailure":
                            prj.SendPublicTweetOnFailure = Value;
                            break;
                        case "SendPrivateTweetOnFailure":
                            prj.SendPrivateTweetOnFailure = Value;
                            break;
                        case "SendTextOnSuccess":
                            prj.SendTextOnSuccess = Value;
                            break;
                        case "SendTextOnFailure":
                            prj.SendTextOnFailure = Value;
                            break;
                    }
                    database.SaveChanges();
                    return Json(new { Success = true });
                }
                else
                {
                    return Json(new { Error = "Project Not Found", Success = false });
                }
            }
            return Json(new { Error = "Something", Success = false });
        }

        [HttpPost]
        [Authorize]
        public JsonResult UpdateMessageTemplate(string Id, string TemplateType, string Value)
        {
            if (null != HttpContext)
            {
                var prj = database.Projects.FirstOrDefault(p => p.User.TwitterUserName == HttpContext.User.Identity.Name && p.ProjectName == Id);

                if (null != prj)
                {
                    switch (TemplateType)
                    {
                        case "Success":
                        case "SuccessTemplate":
                            prj.SuccessTemplate = Value;
                            break;
                        case "Failure":
                        case "FailureTemplate":
                            prj.FailureTemplate = Value;
                            break;
                        default:
                            return Json(new JsonResultModel() { Success = false, Error = "Invalid Template Type (SuccessTemplate, FailureTemplate)" });
                            break;
                    }
                    database.SaveChanges();
                    return Json(new { Success = true });
                }
                else
                {
                    return Json(new { Error = "Project Not Found", Success = false });
                }
            }
            return Json(new { Error = "Something", Success = false });
        }

        [HttpPost]
        [Authorize]
        public JsonResult AddMessageRecipient(string Id, string value, string Type)
        {
            if (null != HttpContext)
            {
                if (!string.IsNullOrEmpty(value.Trim()))
                {
                    var prj = database.Projects.Include("MessageRecipients").Include("TextMessageRecipients").FirstOrDefault(p => p.User.TwitterUserName == HttpContext.User.Identity.Name && p.ProjectName == Id);

                    if (null != prj)
                    {
                        switch (Type)
                        {
                            case "Twitter":
                            case "MessageRecipients":
                                value = value.Replace("@", "").Trim();
                                if (prj.MessageRecipients.FirstOrDefault(m => m.ScreenName == value) == null)
                                {
                                    prj.MessageRecipients.Add(new TwitterMessageRecipient() { ScreenName = value });
                                    database.SaveChanges();
                                    return Json(new { Success = true });
                                }
                                else
                                {
                                    return Json(new JsonResultModel() { Success = false, Error = "That user is already a message recipient" });
                                }
                                break;
                            case "SMS":
                            case "TextMessageRecipients":
                                value = value.Replace("-", "").Trim();
                                if (prj.TextMessageRecipients.FirstOrDefault(m => m.PhoneNumber == value) == null)
                                {
                                    var tmr = database.TextMessageRecipients.FirstOrDefault(f => f.PhoneNumber == value);
                                    if (null == tmr)
                                        tmr = new TextMessageRecipient() { PhoneNumber = value };

                                    prj.TextMessageRecipients.Add(tmr);
                                    try
                                    {
                                        database.SaveChanges();
                                    }
                                    catch (DbEntityValidationException e)
                                    {
                                        return Json(new JsonResultModel() { Error = e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage, Success = false });
                                    }
                                    return Json(new { Success = true });
                                }
                                else
                                {
                                    return Json(new JsonResultModel() { Success = false, Error = "That number is already an SMS recipient" });
                                }
                                break;
                        }

                    }
                    else
                    {
                        return Json(new { Error = "Project Not Found", Success = false });
                    }
                }
                else
                {
                    if (Type == "MessageRecipients")
                    {
                        return Json(new JsonResultModel() { Success = false, Error = "Please provide a Twitter screen name" });
                    }
                    else
                    {
                        return Json(new JsonResultModel() { Success = false, Error = "Please provide a valid phone number including area code" });
                    }
                }
            }
            return Json(new { Error = "Something", Success = false });
        }
        [HttpPost]
        [Authorize]
        public JsonResult RemoveMessageRecipient(string Id, string recipient, string Type)
        {
            if (null != HttpContext)
            {
                var prj = database.Projects.Include("MessageRecipients").Include("TextMessageRecipients").FirstOrDefault(p => p.User.TwitterUserName == HttpContext.User.Identity.Name && p.ProjectName == Id);

                if (null != prj)
                {
                    switch (Type)
                    {
                        case "Twitter":
                        case "MessageRecipients":
                            var el = prj.MessageRecipients.FirstOrDefault(m => m.ScreenName == recipient);
                            if (el != null)
                            {
                                database.MessageRecipients.Remove(el);
                                database.SaveChanges();
                                return Json(new { Success = true });
                            }
                            else
                            {
                                return Json(new JsonResultModel() { Success = false, Error = "That user is not a valid message recipient" });
                            }
                            break;
                        case "SMS":
                        case "TextMessageRecipients":
                            var el2 = prj.TextMessageRecipients.FirstOrDefault(m => m.PhoneNumber == recipient);
                            if (el2 != null)
                            {
                                database.TextMessageRecipients.Remove(el2);
                                database.SaveChanges();
                                return Json(new { Success = true });
                            }
                            else
                            {
                                return Json(new JsonResultModel() { Success = false, Error = "That user is not a valid SMS message recipient" });
                            }
                            break;
                        default:
                            return Json(new { Error = "Invalid Type", Success = false });
                            break;

                    }

                }
                else
                {
                    return Json(new { Error = "Project Not Found", Success = false });
                }
            }
            return Json(new { Error = "Something", Success = false });
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();

        }
    }
}
