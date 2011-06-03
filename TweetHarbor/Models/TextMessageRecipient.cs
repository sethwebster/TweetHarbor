using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TweetHarbor.Models
{
    public class TextMessageRecipient
    {
        [Key]
        public string PhoneNumber { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}