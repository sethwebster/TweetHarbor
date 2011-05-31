using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

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
        }

        [Required]
        [Key]
        public string ProjectName { get; set; }
        public User User { get; set; }
        public bool SendPrivateTweetOnSuccess { get; set; }
        public bool SendPublicTweetOnSuccess { get; set; }
        public bool SendPrivateTweetOnFailure { get; set; }
        public bool SendPublicTweetOnFailure { get; set; }
        public string SuccessTemplate { get; set; }
        public string FailureTemplate { get; set; }

    }
}