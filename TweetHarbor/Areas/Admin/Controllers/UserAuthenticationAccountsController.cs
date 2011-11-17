using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;

namespace TweetHarbor.Areas.Admin.Controllers
{

    [Authorize(Roles = "Admin")]
    public class UserAuthenticationAccountsController : Controller
    {
        ITweetHarborDbContext database;

        public UserAuthenticationAccountsController(ITweetHarborDbContext database)
        {
            this.database = database;
        }

        public ActionResult Index()
        {
            return View(database.UserAuthenticationAccounts.OrderBy(a => a.UserName));
        }

        public ActionResult Delete(int Id)
        {
            database.UserAuthenticationAccounts.Remove(
                database.UserAuthenticationAccounts.FirstOrDefault(u => u.UserAuthenticationAccountId == Id)
                );
            database.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}
