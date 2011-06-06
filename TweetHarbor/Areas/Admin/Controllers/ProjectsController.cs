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
                var user = database.Users.FirstOrDefault(usr => usr.TwitterUserName == HttpContext.User.Identity.Name);
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }

    }
}
