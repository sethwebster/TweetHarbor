using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TweetHarbor.OAuth
{
    public class AppHarborClient
    {
        string clientId = string.Empty;
        public AppHarborClient(string clientId)
        {
            this.clientId = clientId;
        }
        public Uri GetAuthorizationUrl()
        {
            return new Uri(string.Format("https://appharbor.com/user/authorizations/new?client_id={0}", clientId));
        }

        public RedirectResult RedirectToAuthorizationResult()
        {
            return new RedirectResult(GetAuthorizationUrl().ToString());
        }


    }
}