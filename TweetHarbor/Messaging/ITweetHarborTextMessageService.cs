using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweetHarbor.Messaging
{
    public interface ITweetHarborTextMessageService
    {
        bool SendText(string recipient, string textToSend);
    }
}