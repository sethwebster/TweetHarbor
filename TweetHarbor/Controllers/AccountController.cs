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

        Dictionary<string, IOAuthSignInClient> clients;// = new Dictionary<string, IOAuthSignInClient>();

        public AccountController(ITweetHarborDbContext database, ITweetHarborTwitterService twitter, IFormsAuthenticationWrapper Authentication)
        {
            this.database = database;
            this.twitter = twitter;
            this.authentication = Authentication;


        }

        public Dictionary<string, IOAuthSignInClient> Clients
        {
            get
            {
                if (null == clients)
                {
                    clients = new Dictionary<string, IOAuthSignInClient>();

                    string clientIdConfigKey = Request.Url.Host.ToLower() != "tweetharbor.com" ? "AppHarborOAuthClientId" : "AppHarborOAuthClientId.TweetHarbor.com";
                    string secretConfigKey = Request.Url.Host.ToLower() != "tweetharbor.com" ? "AppHarborOAuthSecret" : "AppHarborOAuthSecret.TweetHarbor.com";
                     
                    var clientId = ConfigurationManager.AppSettings[clientIdConfigKey];
                    var secret = ConfigurationManager.AppSettings[secretConfigKey];

                    clients.Add("appharbor", new AppHarborOAuthClient(clientId, secret, database));
                    clients.Add("twitter", new TwitterOAuthClient(twitter, database));

                }
                return clients;

            }
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
                    .Include("AuthenticationAccounts")
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
                    return RedirectToAction("LogIn");
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
        public ActionResult AccountSetup(string Id, User user, string ReturnUrl)
        {
            var dbUser = database.Users.FirstOrDefault(u => u.UniqueId == Id);
            if (null != dbUser && null != user)
            {
                user.UserName = user.UserName != null ? user.UserName.Trim() : null;
                user.EmailAddress = user.EmailAddress != null ? user.EmailAddress.Trim() : null;
                if (string.IsNullOrEmpty(user.UserName))
                    ModelState.AddModelError("UserName", "UserName must not be empty");
                if (string.IsNullOrEmpty(user.EmailAddress))
                    ModelState.AddModelError("EmailAddress", "Please enter an email address");

                var usernameTaken = database.Users.FirstOrDefault(u => u.UserName.ToLower() == user.UserName.ToLower() && u.UserId != dbUser.UserId) != null;
                if (usernameTaken)
                {
                    ModelState.AddModelError("UserName", "That username is already in use");
                }
                if (ModelState.IsValid)
                {
                    dbUser.UserName = user.UserName;
                    dbUser.EmailAddress = user.EmailAddress;
                    database.SaveChanges();
                    //TODO: Add a status update
                    FormsAuthentication.SetAuthCookie(user.UserName, true);
                    if (!string.IsNullOrEmpty(ReturnUrl))
                    {
                        return Redirect(ReturnUrl);
                    }
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
            User dbUser = null;
            if (Request.IsAuthenticated)
            {
                dbUser = database.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            }

            Client = Client ?? "";

            string returnUrl = Request["ReturnUrl"] != null ? "&ReturnUrl=" + Request["ReturnUrl"] : "";
            string redirect = Request.Url.Scheme + "://" +
                Request.Url.Host +
                (Request.Url.Host == "localhost" ? ":" + Request.Url.Port : "") + "/Account/OAuthComplete/"
                + (dbUser != null ? dbUser.UniqueId : "")
                + "?Client=" + Client + "" + returnUrl;

            return Redirect(Clients[Client.ToLower()].GetAuthenticationEndpoint(redirect).AbsoluteUri);
        }

        ///// <summary>
        ///// For twitter
        ///// </summary>
        ///// <param name="oauth_token"></param>
        ///// <param name="oauth_verifier"></param>
        ///// <returns></returns>
        //public ActionResult AuthorizeCallback(string Id, string oauth_token, string oauth_verifier, string denied)
        //{


        //}

        /// <summary>
        /// For AppHb
        /// </summary>
        /// <param name="Code"></param>
        /// <returns></returns>
        public ActionResult OAuthComplete(string Id, string Client, string ReturnUrl)
        {

            var user = Clients[Client.ToLower()].OAuthCallback(Request);
            ActionResult result = null;
            if (null != user)
            {
                // Log the user in
                authentication.SetAuthCookie(user.UserName, true);

                // ensure the unique id has been set
                if (string.IsNullOrEmpty(user.UniqueId))
                {
                    user.UpdateUniqueId();
                    database.SaveChanges();
                }

                // make sure we have a username and email set -- if not, require account setup
                // TODO: Add this check somewhere else -- here they can nav away and we'll only
                // get them when they try again
                if (string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.EmailAddress))
                {
                    result = RedirectToAction("AccountSetup", new { Id = user.UniqueId, ReturnUrl = ReturnUrl });
                }
                else
                {
                    if (string.IsNullOrEmpty(ReturnUrl))
                    {
                        result = RedirectToAction("Index", new { Controller = "Account" });
                    }
                    else
                    {
                        result = Redirect(ReturnUrl);
                    }
                }
            }

            return result;

            string Code = Request["Code"];

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

                    break;
                default:
                    throw new InvalidOperationException("That is not a recognized OAuth client: " + Client);
                    break;
            }
        }


        public ActionResult LogIn()
        {
            return View();
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
