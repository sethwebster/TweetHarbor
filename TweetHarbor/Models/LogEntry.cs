using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweetHarbor.Models
{
    public class LogEntry
    {
        public int LogEntryId { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}