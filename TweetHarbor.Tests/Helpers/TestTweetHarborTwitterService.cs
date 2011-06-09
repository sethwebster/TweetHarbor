using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetHarbor.Messaging;
using TweetSharp;

namespace TweetHarbor.Tests.Helpers
{
    public class TestTweetHarborTwitterService : ITweetHarborTwitterService
    {
        public TweetSharp.TwitterDirectMessage SendDirectMessage(string Id, string Update)
        {
            return new TweetSharp.TwitterDirectMessage() { };
        }

        public TweetSharp.TwitterStatus SendTweet(string Update)
        {
            return new TweetSharp.TwitterStatus() { };
        }

        string token = string.Empty;
        string secret = string.Empty;
        public void AuthenticateWith(string OAuthToken, string OAuthSecret)
        {
            token = OAuthToken;
            secret = OAuthSecret;
        }


        public TweetSharp.OAuthAccessToken GetAccessToken(TweetSharp.OAuthRequestToken token, string verifier)
        {
            OAuthAccessToken ret = new OAuthAccessToken() { Token = token.Token, TokenSecret = token.TokenSecret };
            return ret;
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
            return new TweetSharp.TwitterUser()
            {
                CreatedDate = DateTime.Now.AddDays(-1),
                ScreenName = "TwitterTestServiceUser",
                ProfileImageUrl = "http://a1.notreal.com/a1.jpg",
                Name = "Test Service User"
            };
        }
    }
}
