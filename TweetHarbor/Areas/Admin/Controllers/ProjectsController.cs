using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;

namespace TweetHarbor.Areas.Admin.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        ITweetHarborDbContext database = null;

        public ProjectsController(ITweetHarborDbContext database)
        {
            this.database = database;
        }


        public ActionResult Index()
        {
            if (null != HttpContext)
            {
                var user = database.Users.FirstOrDefault(usr => usr.UserName == HttpContext.User.Identity.Name);
                if (user.IsAdmin)
                {
                    var projects = database.Projects.Include("User").OrderBy(u => u.DateCreated);
                    return View(projects);
                }
                else
                {
                    //TODO: Create better authorization strategy
                    return new RedirectResult("/?f=notauthorized");
                }
            }
            return new EmptyResult();
        }

        public ActionResult ProjectNotifications()
        {
            if (null != HttpContext)
            {
                var user = database.Users.FirstOrDefault(usr => usr.UserName == HttpContext.User.Identity.Name);
                if (user.IsAdmin)
                {
                    var projectNotifications = database.ProjectNotifications.Include("Build").Include("Project.user").Include("Build.commit").OrderByDescending(pn => pn.NotificationDate);
                    return View(projectNotifications);
                }
                else
                {
                    //TODO: Create better authorization strategy
                    return new RedirectResult("/?f=notauthorized");
                }
            }
            return new EmptyResult();
        }
        public ActionResult OutboundNotifications()
        {
            if (null != HttpContext)
            {
                var user = database.Users.FirstOrDefault(usr => usr.UserName == HttpContext.User.Identity.Name);
                if (user.IsAdmin)
                {
                    var projectNotifications = database.OutboundNotifications.OrderByDescending(pn => pn.DateCreated).ThenBy(pn => pn.DateSent);
                    return View(projectNotifications);
                }
                else
                {
                    //TODO: Create better authorization strategy
                    return new RedirectResult("/?f=notauthorized");
                }
            }
            return new EmptyResult();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }

    }
}
