using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace TweetHarbor.Models
{
    public static class EmailHelper
    {
        public static Exception LastError { get; private set; }

        public static bool SendMessage(MailMessage Mail)
        {
            try
            {
                SmtpClient cli = new SmtpClient();
                cli.Send(Mail);
            }
            catch (Exception e)
            {
                LastError = e;
                return false;
            }
            return true;
        }

        public static bool SendMessage(string ToAddress, string ToName, string FromAddress, string FromName, string Subject, string Body, bool IsHtml)
        {
            MailAddress from = new MailAddress(FromAddress, FromName);
            MailAddress to = new MailAddress(ToAddress, ToName);
            MailMessage m = new MailMessage(from, to);
            m.Subject = Subject;
            m.Body = Body;
            m.IsBodyHtml = IsHtml;
            return SendMessage(m);
        }
    }
}