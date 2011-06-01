using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TweetHarbor.Models
{
    public class TwitterMessageRecipient
    {
        [Key]
        public int TwitterMessageRecipientId { get; set; }
        [Required]
        public string ScreenName { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}