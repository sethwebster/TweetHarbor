using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweetHarbor.Models
{
    public class Notification
    {
        public Application application { get; set; }
        public Build build { get; set; }
    }

    public class Application
    {
        public string name { get; set; }
    }

    public class Build
    {
        public Commit commit { get; set; }
        public string status { get; set; }
    }

    public class Commit
    {
        public string id { get; set; }
        public string message { get; set; }
    }
}