using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Models;
using Ninject;
using TweetHarbor.Data;
using TweetHarbor;

namespace System.Web.Mvc
{
    public static class HtmlExtensions
    {
        public static User CurrentUser(this HtmlHelper input)
        {
            User ret = null;
            if (null != HttpContext.Current && null != HttpContext.Current.Request && HttpContext.Current.Request.IsAuthenticated)
            {
                var k = MvcApplication.SharedKernel.Get<ITweetHarborDbContext>();
                ret = k.Users.FirstOrDefault(usr => usr.TwitterUserName == HttpContext.Current.User.Identity.Name);
            }
            return ret;
        }
    }
}