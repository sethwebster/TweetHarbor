using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;
using TweetHarbor.Models;

namespace TweetHarbor.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        ITweetHarborDbContext db;
        public HomeController(ITweetHarborDbContext database)
        {
            db = database;
        }


        public ActionResult FixTwitterAccounts()
        {
            int ct = 0;
            foreach (var u in db.Users.ToArray())
            {
                var oldUser = db.OldUsers.FirstOrDefault(o => o.UniqueId == u.UniqueId);
                if (null != oldUser && u.AuthenticationAccounts.FirstOrDefault(tt => tt.AccountProvider == "twitter") == null)
                {
                    UserAuthenticationAccount acc = new UserAuthenticationAccount()
                    {
                        AccountProvider = "twitter",
                        OAuthToken = oldUser.OAuthToken,
                        OAuthTokenSecret = oldUser.OAuthTokenSecret,
                        ProfilePicUrl = oldUser.UserProfilePicUrl,
                        UserName = oldUser.TwitterUserName
                    };
                    u.AuthenticationAccounts.Add(acc);
                    ct++;
                }
            }
            db.SaveChanges();
            return Content("OK - " + ct);
        }

        public ActionResult Index()
        {
            return View();
        }

    }
}
