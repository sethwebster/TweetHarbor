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
using System.Collections.ObjectModel;
using System.Diagnostics;
using TweetHarbor.OAuth;
using System.Configuration;
using System.Net;

namespace TweetHarbor.Controllers
{
    /// <summary>
    /// Provides the /Account controller
    /// </summary>
    public class AccountController : Controller
    {
        ITweetHarborDbContext database;
        ITweetHarborTwitterService twitter;
        IFormsAuthenticationWrapper authentication;

        public AccountController(ITweetHarborDbContext database, ITweetHarborTwitterService twitter, IFormsAuthenticationWrapper Authentication)
        {
            this.database = database;
            this.twitter = twitter;
            this.authentication = Authentication;
        }

        [Authorize]
        public ActionResult Index()
        {
            if (null != HttpContext)
            {
                if (!string.IsNullOrEmpty(Request["error"]) && Request["error"] == "ImportNotAuthorized")
                {
                    ViewBag.import_error = "Unable to sign in with those credentials";
                }
                ViewBag.UserName = HttpContext.User.Identity.Name;
                var u = database.Users.Include("Projects")
                    .Include("Projects.MessageRecipients")
                    .Include("Projects.TextMessageRecipients")
                    .Include("Projects.ProjectNotifications")
                    .Include("Projects.ProjectNotifications.Build")
                    .Include("Projects.ProjectNotifications.Build.commit")
                    .FirstOrDefault(usr => usr.UserName == HttpContext.User.Identity.Name);

                if (null != u)
                {
                    if (string.IsNullOrEmpty(u.UniqueId))
                    {
                        //TODO: Make this much more secure
                        u.UpdateUniqueId();
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

        public ActionResult AccountSetup(string Id)
        {
            var user = database.Users.FirstOrDefault(u => u.UniqueId == Id);
            if (null != user)
            {
                return View(user);
            }
            else
            {
                //TODO: better to redirect to failed login page?
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult AccountSetup(string Id, User user)
        {
            var dbUser = database.Users.FirstOrDefault(u => u.UniqueId == Id);
            if (null != user)
            {
                user.UserName = user.UserName != null ? user.UserName.Trim() : null;
                user.EmailAddress = user.EmailAddress != null ? user.EmailAddress.Trim() : null;
                if (string.IsNullOrEmpty(user.UserName))
                    ModelState.AddModelError("UserName", "UserName must not be empty");
                if (string.IsNullOrEmpty(user.EmailAddress))
                    ModelState.AddModelError("EmailAddress", "Please enter an email address");
                if (ModelState.IsValid)
                {
                    dbUser.UserName = user.UserName;
                    dbUser.EmailAddress = user.EmailAddress;
                    database.SaveChanges();
                    //TODO: Add a status update
                    FormsAuthentication.SetAuthCookie(user.UserName, true);
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(user);
                }
            }
            else
            {
                //TODO: better to redirect to failed login page?
                return RedirectToAction("Index");
            }
        }

        public ActionResult Authorize(string Client)
        {
            switch (Client.ToLower())
            {
                case "twitter":
                    // Step 1 - Retrieve an OAuth Request Token

#if DEBUG
                    OAuthRequestToken requestToken = twitter.GetRequestToken("http://localhost:9090/Account/AuthorizeCallback"); // <-- The registered callback URL
#else
            OAuthRequestToken requestToken = twitter.GetRequestToken(Properties.Settings.Default.TwitterAuthorizationCallbackUrl); // <-- The registered callback URL
#endif
                    Uri uri = twitter.GetAuthorizationUri(requestToken);
                    return new RedirectResult(uri.ToString(), false /*permanent*/);
                    break;
                case "appharbor":
                    var clientId = ConfigurationManager.AppSettings["AppHarborOAuthClientId"];
                    var secret = ConfigurationManager.AppSettings["AppHarborOAuthSecret"];
                    return new AppHarborClient(clientId, secret).RedirectToAuthorizationResult();
                    break;
                default:
                    throw new ArgumentNullException("Client must be specified");
                    break;


            }
        }

        /// <summary>
        /// For twitter
        /// </summary>
        /// <param name="oauth_token"></param>
        /// <param name="oauth_verifier"></param>
        /// <returns></returns>
        public ActionResult AuthorizeCallback(string oauth_token, string oauth_verifier)
        {
            var requestToken = new OAuthRequestToken { Token = oauth_token };

            // Step 3 - Exchange the Request Token for an Access Token
            OAuthAccessToken accessToken = twitter.GetAccessToken(requestToken, oauth_verifier);
            // Step 4 - User authenticates using the Access Token
            twitter.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

            TwitterUser user = twitter.VerifyCredentials();
            if (null != user)
            {
                var appUser = TwitterCreateOrUpdateAccountIfNeeded(accessToken, user);
                if (string.IsNullOrEmpty(appUser.UserName) || string.IsNullOrEmpty(appUser.EmailAddress))
                {
                    if (string.IsNullOrEmpty(appUser.UniqueId))
                    {
                        appUser.UpdateUniqueId();
                        database.SaveChanges();
                    }
                    return RedirectToAction("AccountSetup", new { Id = appUser.UniqueId });
                }
                else
                {
                    authentication.SetAuthCookie(appUser.UserName, true);
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("OAuthError");
            }
        }

        /// <summary>
        /// For AppHb
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public ActionResult OAuthComplete(string Code, string Client)
        {
            //Workaround for now since AppHb doesn't seem to support passing parameters back
            //UPDATE: Appends extra ?
            if (Client == null)
                Client = Request["?Client"];
            switch (Client.ToLower())
            {

                // We will be moving to using this method later
                case "twitter":
                    throw new NotImplementedException("This path has not been implemented yet");
                    break;
                case "appharbor":
                    var clientId = ConfigurationManager.AppSettings["AppHarborOAuthClientId"];
                    var secret = ConfigurationManager.AppSettings["AppHarborOAuthSecret"];
                    var client = new AppHarborClient(clientId, secret);

                    var token = client.GetAccessToken(Code);
                    var user = client.GetUserInformation(token);

                    var appUser = AppHarborCreateOrUpdateAccountIfNeeded(token, user);
                    if (string.IsNullOrEmpty(appUser.UserName) || string.IsNullOrEmpty(appUser.EmailAddress))
                    {
                        if (string.IsNullOrEmpty(appUser.UniqueId))
                        {
                            appUser.UpdateUniqueId();
                            database.SaveChanges();
                        }
                        return RedirectToAction("AccountSetup", new { Id = appUser.UniqueId });
                    }
                    else
                    {
                        authentication.SetAuthCookie(appUser.UserName, true);
                        return RedirectToAction("Index");
                    }
                    break;
                default:
                    throw new InvalidOperationException("That is not a recognized OAuth client: "+Client);
                    break;
            }
        }

        [NonAction]
        private User TwitterCreateOrUpdateAccountIfNeeded(OAuthAccessToken accessToken, TwitterUser user)
        {
            var returnUser = (from u in database.Users
                              where u.AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "twitter" && ac.UserName == user.ScreenName) != null
                              select u).FirstOrDefault();

            if (null == returnUser) // CREATE
            {
                returnUser = new User();
                returnUser.UserName = "";
                returnUser.EmailAddress = "";
                UserAuthenticationAccount newTwitterAccount = new UserAuthenticationAccount();
                newTwitterAccount.AccountProvider = "twitter";

                newTwitterAccount.UserName = user.ScreenName;
                newTwitterAccount.ProfilePicUrl = user.ProfileImageUrl;
                if (null == returnUser.AuthenticationAccounts)
                    returnUser.AuthenticationAccounts = new Collection<UserAuthenticationAccount>();
                returnUser.AuthenticationAccounts.Add(newTwitterAccount);
                returnUser.UpdateUniqueId();
                database.Users.Add(returnUser);
            }

            returnUser.UserProfilePicUrl = user.ProfileImageUrl;
            var twitterAccount = returnUser.AuthenticationAccounts.First(t => t.AccountProvider == "twitter");
            twitterAccount.OAuthToken = accessToken.Token;
            twitterAccount.OAuthTokenSecret = accessToken.TokenSecret;
            twitterAccount.ProfilePicUrl = user.ProfileImageUrl;

            try
            {
                database.SaveChanges();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception: " + e.Message);
                throw e;
            }
            return returnUser;
        }
        [NonAction]
        private User AppHarborCreateOrUpdateAccountIfNeeded(string AccessToken, AppHarborUser user)
        {
            //TODO: must have some kind of AppHb unique id-- username, etc --see twitter approach (screenname) (for now we used emailaddress)
            var returnUser = (from u in database.Users
                              where u.AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "appharbor" && ac.UserName == user.EmailAddress) != null
                              select u).FirstOrDefault();

            if (null == returnUser) // CREATE
            {
                returnUser = new User();
                returnUser.UserName = user.UserName;
                returnUser.EmailAddress = user.EmailAddress;
                UserAuthenticationAccount newAppHarborAccount = new UserAuthenticationAccount();
                newAppHarborAccount.AccountProvider = "appharbor";
                newAppHarborAccount.UserName = user.UserName;
                newAppHarborAccount.UserName = user.UserName;
                newAppHarborAccount.ProfilePicUrl = "<not implemented>";
                if (null == returnUser.AuthenticationAccounts)
                    returnUser.AuthenticationAccounts = new Collection<UserAuthenticationAccount>();
                returnUser.AuthenticationAccounts.Add(newAppHarborAccount);
                returnUser.UpdateUniqueId();
                database.Users.Add(returnUser);
            }

            //returnUser.UserProfilePicUrl = user.ProfileImageUrl;
            var appharborAccount = returnUser.AuthenticationAccounts.First(t => t.AccountProvider == "appharbor");
            appharborAccount.OAuthToken = AccessToken;
            //appharborAccount.OAuthTokenSecret = accessToken.TokenSecret;
            //appharborAccount.ProfilePicUrl = user.ProfileImageUrl;

            try
            {
                database.SaveChanges();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception: " + e.Message);
                throw e;
            }
            return returnUser;
        }

        public ActionResult LogOff()
        {
            authentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public JsonResult GlobalNotificationToggle(string TweetType, bool Value)
        {
            if (null != HttpContext)
            {
                var u = database.Users.FirstOrDefault(usr => usr.UserName == HttpContext.User.Identity.Name);

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
                        case "SendSMS":
                            u.SendSMS = Value;
                            break;
                    }
                    database.SaveChanges();
                    return Json(new JsonResultModel() { Success = true, Message = "Value has been updated" });
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
                var u = database.Users.FirstOrDefault(usr => usr.UserName == HttpContext.User.Identity.Name);

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
