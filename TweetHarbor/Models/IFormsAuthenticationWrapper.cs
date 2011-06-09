using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetHarbor.Models
{
    public interface IFormsAuthenticationWrapper
    {
        void SetAuthCookie(string Identifier, bool Persist);
        void SignOut();
    }
}
