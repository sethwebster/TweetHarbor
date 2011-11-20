using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;
using TweetHarbor.Models;
using Newtonsoft.Json;
using System.Data.Entity.Validation;
using System.Collections.ObjectModel;
using System.Security;
using System.Net;
using System.IO;
using TweetHarbor.Messaging;
using TweetHarbor.OAuth;
using System.Configuration;

namespace TweetHarbor.Controllers
{
    public class ProjectsController : Controller
    {

        ITweetHarborDbContext database = null;
        ITweetHarborTwitterService twitterService;
        ITweetHarborTextMessageService textMessageService;
        IAppHarborClient appHarborClient;

        public ProjectsController(ITweetHarborDbContext database, ITweetHarborTwitterService twitterService, ITweetHarborTextMessageService textMessageService, IAppHarborClient appHarborClient)
        {
            this.database = database;
            this.twitterService = twitterService;
            this.textMessageService = textMessageService;
            this.appHarborClient = appHarborClient;
        }
        //
        // GET: /Projects/

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ContentResult UserProjects()
        {
            Response.ContentType = "application/json";
            if (null != HttpContext)
            {
                var user = database.Users.Include("Projects")
                 .Include("Projects.MessageRecipients")
                 .Include("Projects.TextMessageRecipients")
                 .Include("Projects.ProjectNotifications")
                 .Include("Projects.ProjectNotifications.Build")
                 .Include("Projects.ProjectNotifications.Build.commit")
                 .FirstOrDefault(usr => usr.UserName == HttpContext.User.Identity.Name);
                
                var data = from p in user.Projects
                           select new
                           {
                               ProjectName = p.ProjectName,
                               ProjectId = p.ProjectId,
                               AppHarborProjectUrl = p.AppHarborProjectUrl,
                               DateCreated = p.DateCreated,
                               SuccessTemplate = p.SuccessTemplate,
                               FailureTemplate = p.FailureTemplate,
                               MessageRecipients = from m in p.MessageRecipients
                                                   select new
                                                   {
                                                       ScreenName = m.ScreenName,
                                                       TwitterMessageRecipientId = m.TwitterMessageRecipientId
                                                   },

                               TextMessageRecipients = from m in p.TextMessageRecipients
                                                       select new
                                                       {
                                                           Name = m.Name,
                                                           PhoneNumber = m.PhoneNumber
                                                       },
                               TwitterAccounts = from t in p.TwitterAccounts
                                                 select new
                                                 {
                                                     ProfilePicUrl = t.ProfilePicUrl,
                                                     UserName = t.UserName,
                                                     UserAuthenticationAccountId = t.UserAuthenticationAccountId
                                                 },
                               ProjectNotifications = from pn in p.ProjectNotifications
                                                      select new
                                                      {
                                                          Build = pn.Build,
                                                          NotificationDate = pn.NotificationDate,
                                                          ProjectNotificationId = pn.ProjectNotificationId
                                                      },
                               SendPrivateTweetOnFailure = p.SendPrivateTweetOnFailure,
                               SendPrivateTweetOnSuccess = p.SendPrivateTweetOnSuccess,
                               SendPublicTweetOnFailure = p.SendPublicTweetOnFailure,
                               SendPublicTweetOnSuccess = p.SendPublicTweetOnSuccess,
                               SendTextOnFailure = p.SendTextOnFailure,
                               SendTextOnSuccess = p.SendTextOnSuccess,

                           };

                JsonSerializerSettings set = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                var ret = JsonConvert.SerializeObject(data, Formatting.None, set);

                return Content(ret);
            }
            return Content(JsonConvert.SerializeObject(new { Error = "Not an HTTP Request" }));
        }

        [Authorize]
        public JsonResult CheckServiceHookUrlStatus(string Id, string Username, string Password)
        {
            if (null != HttpContext)
            {
                var user = database.Users.FirstOrDefault(f => f.UserName == HttpContext.User.Identity.Name);
                ApplicationImporter a = new ApplicationImporter();
                a.AuthenticateAs(Username, Password);
                var projects = a.GetProjects();
                foreach (var p in projects)
                {
                    if (a.DoesServiceHookExist(p.AppHarborProjectUrl, user.GetServiceHookUrl()))
                    {
                        return Json(new JsonResultModel() { Success = true, Message = "OK" });
                    }
                    else
                    {
                        return Json(new JsonResultModel() { Success = true, Message = "NOT SET" });
                    }
                }
            }
            return Json(new { Error = "Not and Http Request" });
        }

        [HttpPost]
        [Authorize]
        public JsonResult ProjectNotificationToggle(string Id, string TweetType, bool Value)
        {
            if (null != HttpContext)
            {
                var prj = database.Projects.FirstOrDefault(p => p.User.UserName == HttpContext.User.Identity.Name && p.ProjectName == Id);

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

        [Authorize]
        public ActionResult TestSuccessfulBuild(int Id)
        {
            var user = database.Users.First(u => u.UserName == User.Identity.Name);
            var project = database.Projects.FirstOrDefault(p => p.ProjectId == Id && user.UserId == p.User.UserId);

            Notification n = new Notification()
            {
                application = new Application()
                {
                    name = project.ProjectName

                },
                build = new Build()
                {
                    commit = new Commit()
                    {
                        id = Guid.NewGuid().ToString(),
                        message = "A successful build test from TweetHarbor"
                    },
                    status = "succeeded"
                }
            };

            NotifyController c = new NotifyController(database, twitterService, textMessageService);
            var res = c.New(user.UserName, user.UniqueId, n);

            //TODO: Make a real post work
            //var notificationValue = JsonConvert.SerializeObject(n);
            //string url = Request.Url.Scheme + "://"
            //    + Request.Url.Host
            //    + (Request.Url.Host.ToLower() == "localhost" ? ":" + Request.Url.Port : "")
            //    + "/notify/new/" + user.UserName + "?token=" + user.UniqueId;

            //WebRequest wr = WebRequest.Create(url);
            //string parms = "notification=" + notificationValue;
            //wr.Method = "POST";
            //byte[] data = System.Text.Encoding.UTF8.GetBytes(parms);
            //wr.ContentLength = data.Length;
            //wr.ContentType = "application/json";

            //var sw = wr.GetRequestStream();
            //sw.Write(data, 0, data.Length);
            //sw.Close();

            //var response = wr.GetResponse();
            //StreamReader sr = new StreamReader(response.GetResponseStream());
            //var val = sr.ReadToEnd();
            //sr.Close();

            return RedirectToAction("Index", new { Controller = "Account" });


        }

        [Authorize]
        public ActionResult TestFailedBuild(int Id)
        {
            var user = database.Users.First(u => u.UserName == User.Identity.Name);
            var project = database.Projects.FirstOrDefault(p => p.ProjectId == Id && user.UserId == p.User.UserId);

            Notification n = new Notification()
            {
                application = new Application()
                {
                    name = project.ProjectName

                },
                build = new Build()
                {
                    commit = new Commit()
                    {
                        id = Guid.NewGuid().ToString(),
                        message = "A successful build test from TweetHarbor"
                    },
                    status = "failed"
                }
            };

            NotifyController c = new NotifyController(database, twitterService, textMessageService);
            var res = c.New(user.UserName, user.UniqueId, n);

            //TODO: Make a real post work
            //var notificationValue = JsonConvert.SerializeObject(n);
            //string url = Request.Url.Scheme + "://"
            //    + Request.Url.Host
            //    + (Request.Url.Host.ToLower() == "localhost" ? ":" + Request.Url.Port : "")
            //    + "/notify/new/" + user.UserName + "?token=" + user.UniqueId;

            //WebRequest wr = WebRequest.Create(url);
            //string parms = "notification=" + notificationValue;
            //wr.Method = "POST";
            //byte[] data = System.Text.Encoding.UTF8.GetBytes(parms);
            //wr.ContentLength = data.Length;
            //wr.ContentType = "application/json";

            //var sw = wr.GetRequestStream();
            //sw.Write(data, 0, data.Length);
            //sw.Close();

            //var response = wr.GetResponse();
            //StreamReader sr = new StreamReader(response.GetResponseStream());
            //var val = sr.ReadToEnd();
            //sr.Close();

            return RedirectToAction("Index", new { Controller = "Account" });


        }

        [HttpPost]
        [Authorize]
        public JsonResult UpdateMessageTemplate(string Id, string TemplateType, string Value)
        {
            if (null != HttpContext)
            {
                var prj = database.Projects.FirstOrDefault(p => p.User.UserName == HttpContext.User.Identity.Name && p.ProjectName == Id);

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
                    var prj = database.Projects.Include("MessageRecipients").Include("TextMessageRecipients").FirstOrDefault(p => p.User.UserName == HttpContext.User.Identity.Name && p.ProjectName == Id);

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
                var prj = database.Projects.Include("MessageRecipients").Include("TextMessageRecipients").FirstOrDefault(p => p.User.UserName == HttpContext.User.Identity.Name && p.ProjectName == Id);

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

        [Authorize]
        public ActionResult ImportFromAppHarbor()
        {
            var user = database.Users.First(u => u.UserName == HttpContext.User.Identity.Name);
            var appHarborAccount = user.AuthenticationAccounts.FirstOrDefault(a => a.AccountProvider == "appharbor");
            if (appHarborAccount != null)
            {
          
                var projects = appHarborClient.GetUserProjects(appHarborAccount.OAuthToken);

                foreach (var p in projects)
                {
                    //TODO: Use a UniqueID rather than name because name can change
                    var userProject = user.Projects.FirstOrDefault(pr => pr.ProjectName == p.ProjectName);
                    if (null == userProject)
                    {
                        userProject = p;
                        user.Projects.Add(userProject);
                    }
                    else
                    {
                        // Update Url
                        userProject.AppHarborProjectUrl = p.AppHarborProjectUrl;
                    }
                    appHarborClient.SetServiceHookUrl(appHarborAccount.OAuthToken, p.ProjectName, "");
                }

                database.SaveChanges();

                return RedirectToAction("Index", new { Controller = "Account" });
            }
            else
            {
                return RedirectToAction("Authorize", new { Controller = "Account", Client = "AppHarbor", ReturnUrl = "/Projects/ImportFromAppHarbor" });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult ImportFromAppHarbor(string Username, string Password)
        {
            if (null != HttpContext)
            {
                var user = database.Users.First(u => u.UserName == HttpContext.User.Identity.Name);
                ApplicationImporter a = new ApplicationImporter();
                try
                {
                    a.AuthenticateAs(Username, Password);
                    var ret = a.GetProjects();
                    foreach (var p in ret)
                    {
                        a.SetProjectServiceHook(p.AppHarborProjectUrl, user.GetServiceHookUrl());
                        user.Projects.Add(p);
                    }
                    database.SaveChanges();
                    return RedirectToAction("Index", "Account");
                }
                catch (Exception e)
                {
                    ViewData["import_error"] = e.Message == "NotAuthorized" ? "We could not log you in with those credentials" : "An error occurred.  Please try again";
                    return RedirectToAction("Index", new { Action = "Index", Controller = "Account", error = "Import" + e.Message });
                }
            }
            return RedirectToAction("Authorize", new { Controller = "Account" });

        }


        [Authorize]
        [HttpPost]
        public ActionResult VerifyServiceHooks(string Username, string Password)
        {
            if (null != HttpContext)
            {
                var user = database.Users.First(u => u.UserName == HttpContext.User.Identity.Name);
                ApplicationImporter a = new ApplicationImporter();
                try
                {
                    a.AuthenticateAs(Username, Password);
                    var ret = a.GetProjects();
                    foreach (var p in ret)
                    {
                        a.SetProjectServiceHook(p.AppHarborProjectUrl, user.GetServiceHookUrl());
                    }
                    return RedirectToAction("Index", "Account");
                }
                catch (Exception e)
                {
                    ViewData["import_error"] = e.Message == "NotAuthorized" ? "We could not log you in with those credentials" : "An error occurred.  Please try again";
                    return RedirectToAction("Index", new { Action = "Index", Controller = "Account", error = "Import" + e.Message });
                }
            }
            return RedirectToAction("Authorize", new { Controller = "Account" });

        }

        [Authorize]
        public ActionResult DeleteAllServiceHooks(string Username, string Password)
        {
            if (null != HttpContext)
            {
                var user = database.Users.First(u => u.UserName == HttpContext.User.Identity.Name);
                if (!user.IsAdmin)
                {
                    throw new SecurityException("Not authorized");
                }

                ApplicationImporter a = new ApplicationImporter();
                a.AuthenticateAs(Username, Password);
                a.DeleteAllServiceHooks();
            }
            return new EmptyResult();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(User user, string ProjectName)
        {
            var projects = database.Projects.Where(p => p.ProjectName == ProjectName && p.User.UserName == user.UserName);
            if (projects.Count() == 0)
            {
                var project = new Project()
                {
                    ProjectName = ProjectName
                };
                user = database.Users.FirstOrDefault(u => u.UserName == user.UserName);
                if (null != user)
                {
                    user.Projects.Add(project);
                }
                database.SaveChanges();
            }
            return RedirectToAction("Index");

        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();

        }
    }
}
