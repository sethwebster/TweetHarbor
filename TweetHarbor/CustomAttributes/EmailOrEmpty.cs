using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataAnnotationsExtensions;

namespace TweetHarbor.CustomAttributes
{
    public class EmailOrEmpty : EmailAttribute
    {
        public EmailOrEmpty()
        {
            this.AllowEmpty = false;
        }
        public bool AllowEmpty { get; set; }
        public override bool IsValid(object value)
        {
            if (AllowEmpty && null == value || string.IsNullOrEmpty(value.ToString()))  
                return true;
            return base.IsValid(value);
        }
    }
}