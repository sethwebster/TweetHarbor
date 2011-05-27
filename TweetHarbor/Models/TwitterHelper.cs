using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweetHarbor.Models
{
    public class TwitterHelper
    {
        public static string ConsumerKey = Properties.Settings.Default.TwitterConsumerKey;
        public static string ConsumerSecret = Properties.Settings.Default.TwitterConsumerSecret;

    }
}