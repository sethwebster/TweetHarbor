using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TweetHarbor.Models
{
    public class User
    {
        [Required]
        [Key]
        public string TwitterUserName { get; set; }
        public string OAuthToken { get; set; }
        public string OAuthTokenSecret { get; set; }
    }
}