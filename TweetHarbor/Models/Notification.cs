using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace TweetHarbor.Models
{
    /// <summary>
    /// Graph which represents the JSON post
    /// </summary>
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
        public Build()
        {
            commit = new Commit();
        }
        [Key]
        public int BuildId { get; set; }
        public Commit commit { get; set; }
        public string status { get; set; }
    }

    public class Commit
    {
        [Key]
        public string id { get; set; }
        public string message { get; set; }
    }
}