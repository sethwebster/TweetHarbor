﻿using System;
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
            this.OutboundNotifications = new Collection<OutboundNotification>();
            this.DateCreated = DateTime.Now;
        }

        [Required]
        [Key]
        public int ProjectId { get; set; }
        [MinLength(1), MaxLength(255), Required]
        public string ProjectName { get; set; }
        public User User { get; set; }
        public bool SendPrivateTweetOnSuccess { get; set; }
        public bool SendPublicTweetOnSuccess { get; set; }
        public bool SendPrivateTweetOnFailure { get; set; }
        public bool SendPublicTweetOnFailure { get; set; }
        public bool SendTextOnSuccess { get; set; }
        public bool SendTextOnFailure { get; set; }
        public string SuccessTemplate { get; set; }
        public string FailureTemplate { get; set; }
        public ICollection<TwitterMessageRecipient> MessageRecipients { get; set; }
        public ICollection<ProjectNotification> ProjectNotifications { get; set; }
        public ICollection<TextMessageRecipient> TextMessageRecipients { get; set; }
        public ICollection<OutboundNotification> OutboundNotifications { get; set; }
        public virtual ICollection<UserAuthenticationAccount> TwitterAccounts { get; set; }
        public DateTime DateCreated { get; set; }
        [MaxLength(400)]
        public string AppHarborProjectUrl { get; set; }

    }
}