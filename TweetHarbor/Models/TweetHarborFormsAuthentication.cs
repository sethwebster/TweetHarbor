using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace TweetHarbor.Models
{
    public class TweetHarborFormsAuthentication : IFormsAuthenticationWrapper
    {

        public void SetAuthCookie(string Identifier, bool Persist)
        {
            FormsAuthentication.SetAuthCookie(Identifier, Persist);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}