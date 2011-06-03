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
        [MinLength(10),MaxLength(10)]
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}