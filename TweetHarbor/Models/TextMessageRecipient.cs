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
        [MinLength(10, ErrorMessage = "The phone number must be a 10 digit phone number including area code"), MaxLength(10, ErrorMessage = "The phone number must be a 10 digit phone number including area code")]
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}