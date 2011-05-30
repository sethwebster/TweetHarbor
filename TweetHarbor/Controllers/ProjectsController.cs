using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;

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
        public JsonResult UpdateNotificationToggle(string Id, string TweetType, bool Value)
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();

        }
    }
}
