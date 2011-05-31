using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TweetSharp;

namespace TweetHarbor.Messaging
{
    public class TweetHarborTwitterServiceOffline : ITweetHarborTwitterService
    {
        const string LOCAL_USER_NAME = "localtestuser";
        TwitterUser user;
        public TweetHarborTwitterServiceOffline()
        {
            user = new TwitterUser();
            user.ScreenName = LOCAL_USER_NAME;
            user.Status = new TwitterStatus() { Text = "This is the only status for the offline user" };
        }
        public TweetSharp.TwitterDirectMessage SendDirectMessage(string Id, string Update)
        {
            return new TweetSharp.TwitterDirectMessage() { };
        }

        public TweetSharp.TwitterStatus SendTweet(string Update)
        {
            return new TwitterStatus() { };
        }

        public void AuthenticateWith(string OAuthToken, string OAuthSecret)
        {
            // Do nada
        }

        public TweetSharp.OAuthAccessToken GetAccessToken(TweetSharp.OAuthRequestToken token, string verifier)
        {
            return new OAuthAccessToken() { ScreenName = LOCAL_USER_NAME, Token = Guid.NewGuid().ToString(), TokenSecret = Guid.NewGuid().ToString() };
        }

        public TweetSharp.OAuthRequestToken GetRequestToken(string callback)
        {
            return new OAuthRequestToken() { Token = Guid.NewGuid().ToString(), TokenSecret = Guid.NewGuid().ToString(), OAuthCallbackConfirmed = true };
        }

        public Uri GetAuthorizationUri(TweetSharp.OAuthRequestToken token)
        {
            var ret = new Uri("/Account/AuthorizeCallback?oauth_token=" + Guid.NewGuid().ToString() + "&oauth_verifier=" + Guid.NewGuid().ToString(), UriKind.Relative);
            return ret;
        }

        public TweetSharp.TwitterUser VerifyCredentials()
        {
            return user;
        }
    }
}