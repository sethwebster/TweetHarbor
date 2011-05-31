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
                var u = database.Users.Include("Projects").FirstOrDefault(usr => usr.TwitterUserName == HttpContext.User.Identity.Name);

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

        public ActionResult LogOn()
        {
            return View();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/LogOff

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
                    if (TestEmailRegex(EmailAddress))
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
        public bool TestEmailRegex(string emailAddress)
        {
            //                string patternLenient = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            //                Regex reLenient = new Regex(patternLenient);
            string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
                  + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                  + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                  + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                  + @"[a-zA-Z]{2,}))$";
            Regex reStrict = new Regex(patternStrict);

            //                      bool isLenientMatch = reLenient.IsMatch(emailAddress);
            //                      return isLenientMatch;

            bool isStrictMatch = reStrict.IsMatch(emailAddress);
            return isStrictMatch;

        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, null, out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }
    }
}
