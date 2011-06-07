using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweetHarbor.Models
{
    public class JsonResultModel
    {
        /// <summary>
        /// A boolean value indicating the operation was a success 
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// The diagnostic error message
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// The status message
        /// </summary>
        public string Message { get; set; }
    }
}