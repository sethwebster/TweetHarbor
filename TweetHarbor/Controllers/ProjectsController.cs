using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;
using TweetHarbor.Models;

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
                            prj.SuccessTemplate = Value;
                            break;
                        case "Failure":
                            prj.FailureTemplate = Value;
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
        public JsonResult AddMessageRecipient(string Id, string value)
        {
            if (null != HttpContext)
            {
                if (!string.IsNullOrEmpty(value.Trim()))
                {
                    value = value.Replace("@", "").Trim();
                    var prj = database.Projects.Include("MessageRecipients").FirstOrDefault(p => p.User.TwitterUserName == HttpContext.User.Identity.Name && p.ProjectName == Id);
                                         
                    if (null != prj)
                    {
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

                    }
                    else
                    {
                        return Json(new { Error = "Project Not Found", Success = false });
                    }
                }
                else
                {
                    return Json(new JsonResultModel() { Success = false, Error = "Please provide a Twitter screen name" });
                }
            }
            return Json(new { Error = "Something", Success = false });
        }
        [HttpPost]
        [Authorize]
        public JsonResult RemoveMessageRecipient(string Id, string recipient)
        {
            if (null != HttpContext)
            {
                var prj = database.Projects.Include("MessageRecipients").FirstOrDefault(p => p.User.TwitterUserName == HttpContext.User.Identity.Name && p.ProjectName == Id);

                if (null != prj)
                {
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
