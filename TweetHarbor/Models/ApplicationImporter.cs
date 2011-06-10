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
        public ICollection<Project> GetProjects(string Username, string Password)
        {
            ICollection<Project> ret = new Collection<Project>();
            HtmlDocument doc = LoadDocument(Username, Password);
            var d = TraverseFor("td", doc.DocumentNode);
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
                }
            }

            return ret;
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

        private HtmlDocument LoadDocument(string Username, string Password)
        {
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

            var cookie = resp.Headers["Set-Cookie"];
            var auth = cookie.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).First();

            req = (HttpWebRequest)WebRequest.Create("https://appharbor.com/application");
            req.CookieContainer = new CookieContainer();
            var cook = new Cookie("authentication", auth.Replace("authentication=", ""));
            cook.Domain = "appharbor.com";
            req.CookieContainer.Add(cook);
            resp = req.GetResponse();
            rs = resp.GetResponseStream();
            HtmlDocument doc = new HtmlDocument();
            doc.Load(resp.GetResponseStream());
            return doc;
        }

        bool ValidateCertificate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}