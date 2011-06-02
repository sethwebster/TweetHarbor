using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using TweetHarbor.Models;
using TweetSharp;
using TweetHarbor.Data;
using System.Text.RegularExpressions;
using TweetHarbor.Messaging;

namespace TweetHarbor.Controllers
{
    /// <summary>
    /// Provides the /Account controller
    /// </summary>
    public class AccountController : Controller
    {
        ITweetHarborDbContext database;
        ITweetHarborTwitterService twitter;

        public AccountController(ITweetHarborDbContext database, ITweetHarborTwitterService twitter)
        {
            this.database = database;
            this.twitter = twitter;
        }

        [Authorize]
        public ActionResult Index()
        {
            if (null != HttpContext)
            {
                ViewBag.UserName = HttpContext.User.Identity.Name;
                var u = database.Users.Include("Projects")
                    .Include("Projects.MessageRecipients")
                    .Include("Projects.ProjectNotifications")
                    .Include("Projects.ProjectNotifications.Build")
                    .FirstOrDefault(usr => usr.TwitterUserName == HttpContext.User.Identity.Name);

                if (null != u)
                {
                    if (string.IsNullOrEmpty(u.UniqueId))
                    {
                        u.UniqueId = u.TwitterUserName.MD5Hash(u.OAuthToken);
                        database.SaveChanges();
                    }
                    return View(u);
                }
                else
                {
                    return RedirectToAction("LogOn");
                }
            }
            return new EmptyResult();
        }

        public ActionResult Authorize()
        {
            // Step 1 - Retrieve an OAuth Request Token

#if DEBUG
            OAuthRequestToken requestToken = twitter.GetRequestToken("http://localhost:9090/Account/AuthorizeCallback"); // <-- The registered callback URL
#else
            OAuthRequestToken requestToken = twitter.GetRequestToken(Properties.Settings.Default.TwitterAuthorizationCallbackUrl); // <-- The registered callback URL
#endif
            Uri uri = twitter.GetAuthorizationUri(requestToken);
            return new RedirectResult(uri.ToString(), false /*permanent*/);
        }

        public ActionResult AuthorizeCallback(string oauth_token, string oauth_verifier)
        {
            var requestToken = new OAuthRequestToken { Token = oauth_token };

            // Step 3 - Exchange the Request Token for an Access Token
            OAuthAccessToken accessToken = twitter.GetAccessToken(requestToken, oauth_verifier);
            // Step 4 - User authenticates using the Access Token
            twitter.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);
            TwitterUser user = twitter.VerifyCredentials();
            ViewBag.Message = string.Format("Your username is {0}", user.ScreenName);
            FormsAuthentication.SetAuthCookie(user.ScreenName, true);

            var appUser = CreateOrUpdateAccountIfNeeded(accessToken, user);

            return RedirectToAction("Index");
        }

        [NonAction]
        private User CreateOrUpdateAccountIfNeeded(OAuthAccessToken accessToken, TwitterUser user)
        {

            var returnUser = database.Users.FirstOrDefault(usr => usr.TwitterUserName == user.ScreenName);
            if (null == returnUser) // CREATE
            {
                returnUser = new User();
                returnUser.TwitterUserName = user.ScreenName;
                returnUser.UniqueId = returnUser.TwitterUserName.MD5Hash(accessToken.Token);
                database.Users.Add(returnUser);
            }

            returnUser.UserProfilePicUrl = user.ProfileImageUrl;
            returnUser.OAuthToken = accessToken.Token;
            returnUser.OAuthTokenSecret = accessToken.TokenSecret;
            try
            {
                database.SaveChanges();
            }
            catch (Exception e)
            {

                throw e;
            }
            return returnUser;
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public JsonResult UpdateTweetToggle(string TweetType, bool Value)
        {
            if (null != HttpContext)
            {
                var u = database.Users.FirstOrDefault(usr => usr.TwitterUserName == HttpContext.User.Identity.Name);

                if (null != u)
                {
                    switch (TweetType)
                    {
                        case "SendPrivateTweet":
                            u.SendPrivateTweet = Value;
                            break;
                        case "SendPublicTweet":
                            u.SendPublicTweet = Value;
                            break;
                    }
                    database.SaveChanges();
                    return Json(new JsonResultModel() { Success = true });
                }
                else
                {
                    return Json(new JsonResultModel() { Error = "User Not Found", Success = false });
                }
            }
            return Json(new JsonResultModel() { Error = "Something", Success = false });
        }

        [Authorize]
        [HttpPost]
        public JsonResult UpdateEmail(string EmailAddress)
        {
            if (null != HttpContext)
            {
                var u = database.Users.FirstOrDefault(usr => usr.TwitterUserName == HttpContext.User.Identity.Name);

                if (null != u)
                {
                    if (EmailAddress.IsEmailAddress())
                    {
                        u.EmailAddress = EmailAddress;
                        database.SaveChanges();
                        return Json(new { Success = true });
                    }
                    else
                    {
                        return Json(new JsonResultModel() { Error = "Please supply a correctly formatted email address", Success = false });
                    }
                }
                else
                {
                    return Json(new JsonResultModel() { Error = "User Not Found", Success = false });
                }
            }
            return Json(new JsonResultModel() { Error = "Something", Success = false });
        }

        //TODO: Add a registration flow -- CODE BELOW is held over from initial creation
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }
    }
}
