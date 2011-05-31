using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetHarbor.Messaging;

namespace TweetHarbor.Tests.Helpers
{
     public class TestTweetHarborTwitterService:ITweetHarborTwitterService
    {
        public TweetSharp.TwitterDirectMessage SendDirectMessage(string Id, string Update)
        {
            return new TweetSharp.TwitterDirectMessage() { };            
        }

        public TweetSharp.TwitterStatus SendTweet(string Update)
        {
            return new TweetSharp.TwitterStatus() { };
        }

        public void AuthenticateWith(string OAuthToken, string OAuthSecret)
        {
            
        }


        public TweetSharp.OAuthAccessToken GetAccessToken(TweetSharp.OAuthRequestToken token, string verifier)
        {
            throw new NotImplementedException();
        }

        public TweetSharp.OAuthRequestToken GetRequestToken(string callback)
        {
            throw new NotImplementedException();
        }

        public Uri GetAuthorizationUri(TweetSharp.OAuthRequestToken token)
        {
            throw new NotImplementedException();
        }

        public TweetSharp.TwitterUser VerifyCredentials()
        {
            throw new NotImplementedException();
        }
    }
}
