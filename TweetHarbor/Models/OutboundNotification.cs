using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TweetHarbor.Models
{
    public class OutboundNotification
    {
        [Key]
        public int NotificationId { get; set; }
        public string NotificationType { get; set; }
        public string Recipient { get; set; }
        public string Message { get; set; }
        public bool SentSuccessfully { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateSent { get; set; }
        public Project Project { get; set; }
    }
}