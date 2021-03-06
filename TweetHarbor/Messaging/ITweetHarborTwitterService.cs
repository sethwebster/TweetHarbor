﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSharp;

namespace TweetHarbor.Messaging
{
    public interface ITweetHarborTwitterService
    {
        TwitterDirectMessage SendDirectMessage(string Id, string Update);
        TwitterStatus SendTweet(string Update);
        void AuthenticateWith(string OAuthToken, string OAuthSecret);
        OAuthAccessToken GetAccessToken(OAuthRequestToken token, string verifier);
        OAuthRequestToken GetRequestToken(string callback);
        Uri GetAuthorizationUri(OAuthRequestToken token);
        TwitterUser VerifyCredentials();
    }
}
