using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using DataAnnotationsExtensions;
using TweetHarbor.CustomAttributes;

namespace TweetHarbor.Models
{
    public class User
    {
        public User()
        {
            this.Projects = new Collection<Project>();
            this.AuthenticationAccounts = new Collection<UserAuthenticationAccount>();
            this.DateCreated = DateTime.Now;
        }

        [Required]
        [Key]
        public int UserId { get; set; }
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        public string UniqueId { get; set; }
        [MaxLength(255)]
        [Display(Name = "Email Address")]
        [EmailOrEmpty]
        public string EmailAddress { get; set; }
        public string UserProfilePicUrl { get; set; }
        public bool SendPrivateTweet { get; set; }
        public bool SendPublicTweet { get; set; }
        public bool SendSMS { get; set; }
        public ICollection<Project> Projects { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual ICollection<UserAuthenticationAccount> AuthenticationAccounts { get; set; }

        public void UpdateUniqueId()
        {
            //TODO: Make this far more random
            this.UniqueId = this.UserName.MD5Hash(DateTime.Now.Ticks.ToString());
        }

        public string GetServiceHookUrl()
        {
            return string.Format("http://tweetharbor.apphb.com/notify/new/{0}?token={1}", this.UserName, this.UniqueId);
        }
    }
}