using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

namespace TweetHarbor.Models
{
    public class OldUser
    {
        public OldUser()
        {
            this.Projects = new Collection<Project>();
            this.DateCreated = DateTime.Now;
        }
        [Required]
        [Key]
        [MinLength(2), MaxLength(255)]
        public string TwitterUserName { get; set; }
        [Required]
        [MinLength(2), MaxLength(255)]
        public string OAuthToken { get; set; }
        [Required]
        [MinLength(2), MaxLength(255)]
        public string OAuthTokenSecret { get; set; }
        [Required]
        [MinLength(2), MaxLength(255)]
        public string UniqueId { get; set; }
        [MaxLength(255)]
        public string EmailAddress { get; set; }
        public string UserProfilePicUrl { get; set; }
        public bool SendPrivateTweet { get; set; }
        public bool SendPublicTweet { get; set; }
        public bool SendSMS { get; set; }
        public ICollection<Project> Projects { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime DateCreated { get; set; }

        public string GetServiceHookUrl()
        {
            return string.Format("http://tweetharbor.apphb.com/notify/new/{0}?token={1}", this.TwitterUserName, this.UniqueId);
        }
    }
}