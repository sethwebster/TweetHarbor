using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace TweetHarbor.Tests.Helpers
{
    public class TestHttpRequestBase : HttpRequestBase
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        public void Set(string Key, string Value)
        {
            values[Key] = Value;
        }
        public override string this[string key]
        {
            get
            {
                return values[key].ToString();
            }
        }

        public override void ValidateInput()
        {

        }
    }
}
