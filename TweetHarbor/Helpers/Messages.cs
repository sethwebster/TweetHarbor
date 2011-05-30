using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web.Mvc
{
    public static class Messages
    {
        public static string DefaultSuccessTemplate { get { return TweetHarbor.Properties.Settings.Default.DefaultSuccessTemplate; } }
        public static string DefaultFailureTemplate { get { return TweetHarbor.Properties.Settings.Default.DefaultFailureTemplate; } }
    }
}