using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using TweetHarbor.Models;

namespace TweetHarbor.Controllers
{
    public class NotifyController : Controller
    {
        public ActionResult New()
        {
            return Redirect("/?f=newnotify");
        }

        [HttpPost]
        public JsonResult New(string Id, string token, Notification notification)
        {
            using (var db = new TweetHarborDbContext())
            {
                var u = db.Users.FirstOrDefault(usr => usr.TwitterUserName == Id && usr.UniqueId == token);
                if (null != u)
                {
                    TweetSharp.TwitterService s = new TweetSharp.TwitterService(TwitterHelper.ConsumerKey, TwitterHelper.ConsumerSecret);
                    s.AuthenticateWith(u.OAuthToken, u.OAuthTokenSecret);
                    s.SendDirectMessage(Id, string.Format("Application {0} build {1}, {2}", notification.application.name, notification.build.status, notification.build.commit.message));

                    return Json(notification);
                }
                else
                {
                    return Json(new { Error = "NotAuthorized" });
                }

            }
        }
    }
}
