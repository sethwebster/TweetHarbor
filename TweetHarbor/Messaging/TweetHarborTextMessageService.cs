using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using TweetHarbor.Messaging.Twilio;
using System.Collections;

namespace TweetHarbor.Messaging
{
    public class TweetHarborTextMessageService : ITweetHarborTextMessageService
    {
        // Twilio REST API version
        const string API_VERSION = "2010-04-01";

        public bool SendText(string recipient, string textToSend)
        {
            // Twilio AccountSid and AuthToken
            string TwilioAccountSID = ConfigurationManager.AppSettings["TwilioSID"];
            string ACCOUNT_TOKEN = ConfigurationManager.AppSettings["TwilioToken"];
            string CallerId = ConfigurationManager.AppSettings["TwilioCallerId"];

            Account account;
            Hashtable h;

            // Create Twilio REST account object using Twilio account ID and token
            account = new Account(TwilioAccountSID, ACCOUNT_TOKEN);

            h = new Hashtable();
            h.Add("From", CallerId);
            h.Add("To", recipient);
            h.Add("Body", textToSend);
            try
            {
                var res = account.request(String.Format("/{0}/Accounts/{1}/SMS/Messages",
                     API_VERSION, TwilioAccountSID), "POST", h);
                return true;
            }
            catch (TwilioRestException e)
            {
                throw e;
            }
            return false;

        }
    }
}