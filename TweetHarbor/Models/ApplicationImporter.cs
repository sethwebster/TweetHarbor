using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Collections.Specialized;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using HtmlAgilityPack;

namespace TweetHarbor.Models
{
    public class ApplicationImporter : IApplicationImporter
    {
        Cookie authenticationCookie;
        User user;
        public ICollection<Project> GetProjects(string Username, string Password, User user)
        {
            this.user = user;
            ICollection<Project> ret = new Collection<Project>();
            var doc = LoadDocument(Username, Password);
            if (doc.Success)
            {
                var d = TraverseFor("td", doc.Document.DocumentNode);
                foreach (var n in d)
                {
                    var anchors = TraverseFor("a", n);
                    if (anchors.Count() > 0 && anchors.First().Attributes["href"].Value.Contains("application"))
                    {
                        // winner
                        var proj = new Project()
                        {
                            ProjectName = anchors.First().InnerHtml
                        };
                        ret.Add(proj);
                        try
                        {
                            SetServiceHook(proj, anchors.First().Attributes["href"].Value);
                        }
                        catch (Exception e)
                        {

                        }

                    }
                }
                return ret;
            }
            else
            {
                throw new Exception(doc.Error);
            }
        }

        private void SetServiceHook(Project proj, string projectUrl)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(projectUrl + "/servicehook");
            req.AllowAutoRedirect = false;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(authenticationCookie);
            var c = new Cookie("ASP.NET_SessionId", "h2z5gag4ml5fxpodm515eirn");
            c.Domain = "appharbor.com";

            req.CookieContainer.Add(c);
            var sw = new StreamWriter(req.GetRequestStream());
            sw.Write(string.Format("Url=http://tweetharbor.apphb.com/notify/new/{0}?token={1}", user.TwitterUserName, user.UniqueId));
            sw.Flush();
            var resp = req.GetResponse();
            var sr = new StreamReader(resp.GetResponseStream());
            var str = sr.ReadToEnd();

        }

        IEnumerable<HtmlNode> TraverseFor(string TagName, HtmlNode startingNode)
        {
            List<HtmlNode> ret = new List<HtmlNode>();
            foreach (var n in startingNode.ChildNodes)
            {
                if (n.Name == TagName)
                {
                    ret.Add(n);
                }
                ret.AddRange(TraverseFor(TagName, n));
            }
            return ret;
        }

        private LoadDocumentResult LoadDocument(string Username, string Password)
        {
            LoadDocumentResult ret = new LoadDocumentResult();
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(ValidateCertificate);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://appharbor.com/session");
            req.AllowAutoRedirect = false;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            var sw = new StreamWriter(req.GetRequestStream());
            sw.Write("UserIdentifier=" + Username + "&Password=" + Password);
            sw.Flush();
            var resp = req.GetResponse();
            var rs = resp.GetResponseStream();
            string cookie = string.Empty;
            if (resp.Headers.AllKeys.Contains("Set-Cookie"))
            {
                cookie = resp.Headers["Set-Cookie"];
                var auth = cookie.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).First();

                req = (HttpWebRequest)WebRequest.Create("https://appharbor.com/application");
                req.CookieContainer = new CookieContainer();
                authenticationCookie = new Cookie("authentication", auth.Replace("authentication=", ""));
                authenticationCookie.Domain = "appharbor.com";
                req.CookieContainer.Add(authenticationCookie);
                resp = req.GetResponse();
                rs = resp.GetResponseStream();
                HtmlDocument doc = new HtmlDocument();
                doc.Load(resp.GetResponseStream());
                ret.Document = doc;
                ret.Success = true;

            }
            else
            {
                ret.Success = false;
                ret.Error = "NotAuthorized";
            }
            return ret;
        }

        bool ValidateCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public class LoadDocumentResult
        {
            public HtmlDocument Document { get; set; }
            public bool Success { get; set; }
            public string Error { get; set; }
        }
    }
}