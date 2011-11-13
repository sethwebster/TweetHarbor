using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TweetHarbor.Models
{
    public class UserAuthenticationAccount
    {
        [Required]
        [Key]
        public int UserAuthenticationAccountId { get; set; }
        [Required]
        public virtual User User { get; set; }

        [Required]
        public string UserName { get; set; }
        [Required]
        public string AccountProvider { get; set; } // APPHarbor, OpenId, Twitter, Etc

        public virtual ICollection<Project> Projects { get; set; }

        public string OAuthTokenSecret { get; set; }
        public string OAuthToken { get; set; }
        public string ProfilePicUrl { get; set; }
    }
}
