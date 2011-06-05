using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TweetHarbor.Models
{
    public class ProjectNotification
    {
        public ProjectNotification()
        {
            this.NotificationDate = DateTime.Now;
        }
    
        [Key]
        public int ProjectNotificationId { get; set; }
        public Project Project { get; set; }
        public Build Build { get; set; }
        public DateTime NotificationDate { get; set; }

    }
}