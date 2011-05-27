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
    }
}