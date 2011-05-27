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
        [Required]
        public string OAuthToken { get; set; }
        [Required]
        public string OAuthTokenSecret { get; set; }
        [Required]
        public string UniqueId { get; set; }
        public string EmailAddress { get; set; }
        public string UserProfilePicUrl { get; set; }
    }
}