using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using TweetHarbor.Models;

namespace TweetHarbor.OAuth
{
    public interface IOAuthSignInClient
    {
        string Name { get; }
        Uri GetAuthenticationEndpoint(string returnToUrl);
        User OAuthCallback(HttpRequestBase Request);
            
    }
}
