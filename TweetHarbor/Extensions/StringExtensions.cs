using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace TweetHarbor
{
    public static class StringExtensions
    {
        public static bool IsEmailAddress(this string input)
        {
            //                string patternLenient = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            //                Regex reLenient = new Regex(patternLenient);
            string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
                  + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                  + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                  + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                  + @"[a-zA-Z]{2,}))$";
            Regex reStrict = new Regex(patternStrict);

            //                      bool isLenientMatch = reLenient.IsMatch(emailAddress);
            //                      return isLenientMatch;

            bool isStrictMatch = reStrict.IsMatch(input);
            return isStrictMatch;
        }
    }
}