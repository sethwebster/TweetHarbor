using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TweetHarbor.Messaging;
using TweetSharp;
using TweetHarbor.Data;
using TweetHarbor.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace TweetHarbor.OAuth
{
    public class TwitterOAuthClient : IOAuthSignInClient
    {
        ITweetHarborTwitterService twitterService;
        ITweetHarborDbContext database;

        public TwitterOAuthClient(ITweetHarborTwitterService service, ITweetHarborDbContext database)
        {
            this.twitterService = service;
            this.database = database;
        }

        public string Name
        {
            get { return "twitter"; }
        }

        public Uri GetAuthenticationEndpoint(string returnToUrl)
        {
           OAuthRequestToken token = twitterService.GetRequestToken(returnToUrl);
            return twitterService.GetAuthorizationUri(token);
        }

        Models.User IOAuthSignInClient.OAuthCallback(HttpRequestBase Request)
        {
            if (string.IsNullOrEmpty(Request["denied"]))
            {
                var requestToken = new OAuthRequestToken { Token = Request["oauth_token"] };

                // Step 3 - Exchange the Request Token for an Access Token
                OAuthAccessToken accessToken = twitterService.GetAccessToken(requestToken, Request["oauth_verifier"]);
                // Step 4 - User authenticates using the Access Token
                twitterService.AuthenticateWith(accessToken.Token, accessToken.TokenSecret);

                TwitterUser user = twitterService.VerifyCredentials();
                if (null != user)
                {
                    // We are adding an account to this user
                    User masterUser = null;
                    if (!string.IsNullOrEmpty(Request["Id"]))
                    {
                        masterUser = database.Users.FirstOrDefault(u => u.UniqueId == Request["Id"]);
                    }
                    var appUser = TwitterCreateOrUpdateAccountIfNeeded(accessToken, user, masterUser);
                    return appUser;
                }
                else
                {
                    //TODO: we should probably throw some kind of authentication error here
                    return null;
                }
            }
            else
            {
                //TODO: Should we really return null here? This request was denied.
                return null;
            }
        }

        private void MergeUsers(User destinationUser, User fromUser, TwitterUser user)
        {
            var authAccount = fromUser.AuthenticationAccounts.FirstOrDefault(t => t.AccountProvider == "twitter" && t.UserName == user.ScreenName);
            destinationUser.AuthenticationAccounts.Add(authAccount);
            fromUser.AuthenticationAccounts.Remove(authAccount);

            foreach (var p in fromUser.Projects)
            {
                destinationUser.Projects.Add(p);

            }
            fromUser.Projects.Clear();
        }

        private User TwitterCreateOrUpdateAccountIfNeeded(OAuthAccessToken accessToken, TwitterUser user, User returnUser)
        {
            // If not passed a user, let's query to find out if
            // we already have a master user for this twitter user
            if (null == returnUser)
            {
                returnUser = (from u in database.Users
                              where u.AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "twitter" && ac.UserName == user.ScreenName) != null
                              select u).FirstOrDefault();
            }
            else
            {
                var otherUser = (from u in database.Users
                                 where u.AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "twitter" && ac.UserName == user.ScreenName) != null
                                 && u.UserId != returnUser.UserId
                                 select u).FirstOrDefault();

                if (null != otherUser)
                {
                    // This twitter account is owned by another user
                    // we need to merge the data
                    MergeUsers(returnUser, otherUser, user);
                }
            }

            // If we're still short a user account, we will create one here
            if (null == returnUser) // CREATE
            {
                returnUser = new User();
                returnUser.UserName = user.ScreenName;
                returnUser.EmailAddress = "";
                returnUser.UpdateUniqueId();
                database.Users.Add(returnUser);
            }

            // Now we will pull our actual twitter account, it if exists
            // If it doesn't, we will create it
            UserAuthenticationAccount twitterAccount = returnUser.
                AuthenticationAccounts.FirstOrDefault(a => a.AccountProvider == "twitter" && a.UserName == user.ScreenName);
            if (twitterAccount == null)
            {
                twitterAccount = new UserAuthenticationAccount();
                twitterAccount.AccountProvider = "twitter";

                twitterAccount.UserName = user.ScreenName;
                twitterAccount.ProfilePicUrl = user.ProfileImageUrl;
                if (null == returnUser.AuthenticationAccounts)
                    returnUser.AuthenticationAccounts = new Collection<UserAuthenticationAccount>();
                returnUser.AuthenticationAccounts.Add(twitterAccount);

            }

            // We'll update some information here
            if (string.IsNullOrEmpty(returnUser.UserProfilePicUrl))
                returnUser.UserProfilePicUrl = user.ProfileImageUrl;
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


    }
}