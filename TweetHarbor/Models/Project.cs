using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

namespace TweetHarbor.Models
{
    public class Project
    {
       
        public Project()
        {
            SendPrivateTweetOnFailure = true;
            SendPrivateTweetOnSuccess = true;
            SendPublicTweetOnFailure = false;
            SendPublicTweetOnSuccess = false;
            SendTextOnFailure = false;
            SendTextOnFailure = false;
            this.MessageRecipients = new Collection<TwitterMessageRecipient>();
            this.ProjectNotifications = new Collection<ProjectNotification>();
            this.TextMessageRecipients = new Collection<TextMessageRecipient>();
        }

        [Required]
        [Key]
        public string ProjectName { get; set; }
        public User User { get; set; }
        public bool SendPrivateTweetOnSuccess { get; set; }
        public bool SendPublicTweetOnSuccess { get; set; }
        public bool SendPrivateTweetOnFailure { get; set; }
        public bool SendPublicTweetOnFailure { get; set; }
        public bool SendTextOnSuccess {get;set;}
        public bool SendTextOnFailure {get;set;}
        public string SuccessTemplate { get; set; }
        public string FailureTemplate { get; set; }
        public ICollection<TwitterMessageRecipient> MessageRecipients { get; set; }
        public ICollection<ProjectNotification> ProjectNotifications { get; set; }
        public ICollection<TextMessageRecipient> TextMessageRecipients { get; set; }

    }
}