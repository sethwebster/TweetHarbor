using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;
using System.Web.Security;
using TweetHarbor.Models;

namespace TweetHarbor.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        ITweetHarborDbContext database = null;

        public UsersController(ITweetHarborDbContext database)
        {
            this.database = database;
        }

        public ActionResult Impersonate(string Id)
        {
            var user = database.Users.FirstOrDefault(u => u.UniqueId == Id);
            FormsAuthentication.SetAuthCookie(user.UserName, false);
            return RedirectToAction("Index", new { Controller = "Account", Area = "" });
        }

        public ActionResult Index()
        {
            if (null != HttpContext)
            {
                var user = database.Users.FirstOrDefault(usr => usr.UserName == HttpContext.User.Identity.Name);
                if (user.IsAdmin)
                {
                    var users = database.Users
                        .Include("Projects")
                        .Include("AuthenticationAccounts")
                        .OrderBy(u => u.UserName).OrderByDescending(y => y.DateCreated);
                    return View(users);
                }
                else
                {
                    //TODO: Create better authorization strategy
                    return new RedirectResult("/?f=notauthorized");
                }
            }
            return new EmptyResult();
        }

        public ActionResult Edit(string Id)
        {
            var user = database.Users.FirstOrDefault(u => u.UniqueId == Id);
            if (null != user)
            {
                return View(user);
            }
            return RedirectToAction("NotFound");
        }

        [HttpPost]
        public ActionResult Edit(string Id, User user)
        {
            database.Users.Attach(user);
            (database as System.Data.Entity.DbContext).Entry(user).State = System.Data.EntityState.Modified;
            database.SaveChanges();
            return View(user);
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult Delete(string Id)
        {
            var user = database.Users.FirstOrDefault(u => u.UniqueId == Id);
            if (user.Projects.Count == 0)
            {
                database.Users.Remove(user);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            return RedirectToAction("index", new { error = "Users has projects attached.  Those must be deleted first." });
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }

    }
}
