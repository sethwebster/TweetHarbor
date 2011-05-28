using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using TweetHarbor.Models;
using TweetHarbor.Data;

namespace TweetHarbor.Controllers
{
    public class NotifyController : Controller
    {
        ITweetHarborDbContext database;

        public NotifyController(ITweetHarborDbContext database)
        {
            this.database = database;
        }

        public ActionResult New()
        {
            return Redirect("/?f=newnotify");
        }

        [HttpPost]
        public JsonResult New(string Id, string token, Notification notification)
        {
            var u = database.Users.FirstOrDefault(usr => usr.TwitterUserName == Id && usr.UniqueId == token);
            if (null != u)
            {
                TweetSharp.TwitterService s = new TweetSharp.TwitterService(TwitterHelper.ConsumerKey, TwitterHelper.ConsumerSecret);
                s.AuthenticateWith(u.OAuthToken, u.OAuthTokenSecret);
                var stringUpdate = string.Format("Application {0} build {1}, {2}", notification.application.name, notification.build.status, notification.build.commit.message);
                if (stringUpdate.Length > 140)
                    stringUpdate = stringUpdate.Substring(0, 136) + "...";
                if (u.SendPrivateTweet)
                {
                    var dmRes = s.SendDirectMessage(u.TwitterUserName, stringUpdate);
                }
                if (u.SendPublicTweet)
                {
                    var pubRes = s.SendTweet(stringUpdate);
                }
                return Json(notification);
            }
            else
            {
                return Json(new { Error = "NotAuthorized" });
            }

        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }
    }
}
