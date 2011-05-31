using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TweetSharp;

namespace TweetHarbor.Messaging
{
    public class TweetHarborTwitterService : ITweetHarborTwitterService
    {
        private TwitterService service;

        public TweetHarborTwitterService(string ConsumerKey, string ConsumerSecret)
        {
            service = new TwitterService(ConsumerKey, ConsumerSecret);
        }



        public TwitterDirectMessage SendDirectMessage(string Id, string Update)
        {
            return service.SendDirectMessage(Id, Update);
        }

        public TwitterStatus SendTweet(string Update)
        {
            return service.SendTweet(Update);
        }

        public void AuthenticateWith(string OAuthToken, string OAuthSecret)
        {
            service.AuthenticateWith(OAuthToken, OAuthSecret);
        }
    }
}