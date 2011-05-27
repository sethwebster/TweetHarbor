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
        [HttpPost]
        public JsonResult New(string Id, dynamic payload)
        {
            using (var db = new TweetHarborDbContext())
            {
                var u = db.Users.FirstOrDefault(usr => usr.TwitterUserName == Id);
                if (null != u)
                {

                    TweetSharp.TwitterService s = new TweetSharp.TwitterService(TwitterHelper.ConsumerKey, TwitterHelper.ConsumerSecret);
                    s.AuthenticateWith(u.OAuthToken, u.OAuthTokenSecret);
                    var status = (string)payload.status;
                    s.SendDirectMessage(Id, string.Format("Application {0} build {1}, {2}",payload.application.name,payload.build.status,payload.build.commit.message));

                    return Json(payload);
                }
                else
                {
                    return Json(new { Error = "NotAuthorized" });
                }

            }
        }
    }
}
