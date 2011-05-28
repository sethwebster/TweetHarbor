using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TweetHarbor.Models
{
    public class Project
    {
        [Required]
        [Key]
        public string ProjectName { get; set; }
        public User User { get; set; }
        public bool SendPrivateTweetOnSuccess { get; set; }
        public bool SendPublicTweetTweetOnSuccess { get; set; }
        public bool SendPrivateTweetOnFailure { get; set; }
        public bool SendPublicTweetTweetOnFailure { get; set; }
        public bool SuccessTemplate { get; set; }
        public bool FailureTemplate { get; set; }
        
    }
}