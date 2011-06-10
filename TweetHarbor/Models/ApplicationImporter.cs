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

        public ApplicationImporter()
        {
            ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(ValidateCertificate);

        }

        public bool AuthenticateAs(string Username, string Password)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://appharbor.com/session");
            req.AllowAutoRedirect = false;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            var sw = new StreamWriter(req.GetRequestStream());
            sw.Write("UserIdentifier=" + Username + "&Password=" + Password);
            sw.Flush();
            var resp = req.GetResponse();
            using (var rs = resp.GetResponseStream())
            {
                string cookie = string.Empty;
                if (resp.Headers.AllKeys.Contains("Set-Cookie"))
                {
                    cookie = resp.Headers["Set-Cookie"];
                    var auth = cookie.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).First();
                    authenticationCookie = new Cookie("authentication", auth.Replace("authentication=", ""));
                    authenticationCookie.Domain = "appharbor.com";
                    return true;
                }
                return false;
            }
        }

        public bool SetProjectServiceHook(string ProjectUrl, string ServiceHookUrl)
        {
            ThrowIfNotAuthenticated();
            try
            {
                if (!DoesServiceHookExist(ProjectUrl, ServiceHookUrl))
                {
                    SetServiceHook(ProjectUrl, ServiceHookUrl);
                }
                return true;
            }
            catch (Exception e)
            {
                //TODO: capture this
                return false;
            }

        }

        private bool DeleteProjectServiceHook(string ProjectUrl, string ServiceHookUrl, bool RequireMatch)
        {
            ThrowIfNotAuthenticated();

            HttpWebRequest req = GetAuthenticatedWebRequest(ProjectUrl + "/servicehook");
            req.AllowAutoRedirect = false;
            req.Method = "GET";
            var resp = req.GetResponse();
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(sr);

                var forms = TraverseFor("form", doc.DocumentNode);
                foreach (var f in forms)
                {
                    if (f.ParentNode.Name == "td")
                    {
                        HtmlNode prevTd = f.ParentNode.PreviousSibling;

                        while (prevTd.Name != "td" && prevTd != null)
                            prevTd = prevTd.PreviousSibling;

                        if (prevTd.InnerHtml.Trim() == ServiceHookUrl ||
                            // Close enough match (if RequireMatch is false)
                            (RequireMatch == false && prevTd.InnerHtml.Contains("tweetharbor")))
                        {
                            req = GetAuthenticatedWebRequest("https://appharbor.com" + f.Attributes["action"].Value);
                            req.ContentType = "application/x-www-form-urlencoded";
                            req.Method = "POST";
                            req.Headers.Add("Origin", "https://appharbor.com");
                            using (var sw2 = new StreamWriter(req.GetRequestStream()))
                            {
                                sw2.Write("_method=delete");
                                sw2.Flush();
                                req.GetResponse();
                            }
                        }
                    }
                }
                //TODO: Figure out how to determine if this really worked
                return true;
            }

        }

        public bool DeleteProjectServiceHook(string ProjectUrl, string ServiceHookUrl)
        {
            return DeleteProjectServiceHook(ProjectUrl, ServiceHookUrl, true);
        }

        public bool SetAllProjectServiceHooks(string ServiceHookUrl)
        {
            ThrowIfNotAuthenticated();
            var projs = GetProjects();
            foreach (var p in projs)
            {
                SetServiceHook(p.AppHarborProjectUrl, ServiceHookUrl);
            }

            //TODO: Determine the best approach to knowing whether this worked or not
            return true;
        }

        public bool DeleteAllServiceHooks()
        {
            ThrowIfNotAuthenticated();
            var projs = GetProjects();
            foreach (var p in projs)
            {
                try
                {
                    DeleteProjectServiceHook(p.AppHarborProjectUrl, null, false);
                }
                catch (Exception e)
                {
                    //TODO: Capture these failures
                }
            }

            //TODO: Determine the best approach to knowing whether this worked or not
            return true;
        }

        public ICollection<Project> GetProjects()
        {
            ThrowIfNotAuthenticated();
            ICollection<Project> ret = new Collection<Project>();
            var doc = LoadDocument();
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
                            ProjectName = anchors.First().InnerHtml,
                            AppHarborProjectUrl = anchors.First().Attributes["href"].Value
                        };
                        ret.Add(proj);
                    }
                }
                return ret;
            }
            else
            {
                throw new Exception(doc.Error);
            }
        }

        private void SetServiceHook(string projectUrl, string ServiceHookUrl)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(projectUrl + "/servicehook");
            req.AllowAutoRedirect = false;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(authenticationCookie);

            using (var sw = new StreamWriter(req.GetRequestStream()))
            {
                sw.Write("Url=" + ServiceHookUrl);
                sw.Flush();
                var resp = req.GetResponse();
                using (var sr = new StreamReader(resp.GetResponseStream()))
                {
                    var str = sr.ReadToEnd();
                    //TODO: Figure out a way to validate that we were successful
                }
            }
        }

        public bool DoesServiceHookExist(string projectUrl, string ServiceHookUrl)
        {
            ThrowIfNotAuthenticated();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(projectUrl + "/servicehook");
            req.AllowAutoRedirect = false;
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "GET";
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(authenticationCookie);
            using (var resp = req.GetResponse())
            {
                using (var sr = new StreamReader(resp.GetResponseStream()))
                {
                    var str = sr.ReadToEnd();
                    return str.Contains(ServiceHookUrl);
                }
            }

        }

        IEnumerable<HtmlNode> TraverseFor(string TagName, HtmlNode startingNode)
        {
            //TODO: Convert to use yield
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

        HttpWebRequest GetAuthenticatedWebRequest(string Url)
        {
            HttpWebRequest ret = (HttpWebRequest)WebRequest.Create(Url);
            ret.CookieContainer = new CookieContainer();
            ret.CookieContainer.Add(authenticationCookie);
            return ret;
        }

        LoadDocumentResult LoadDocument()
        {
            ThrowIfNotAuthenticated();
            LoadDocumentResult ret = new LoadDocumentResult();

            HttpWebRequest req = GetAuthenticatedWebRequest("https://appharbor.com/application");
            var resp = req.GetResponse();
            using (var rs2 = resp.GetResponseStream())
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(rs2);
                ret.Document = doc;
                ret.Success = true;
            }
            return ret;
        }
        private void ThrowIfNotAuthenticated()
        {
            if (null == authenticationCookie)
                throw new Exception("The request is not authenticated.  Please call AuthenticateAs first");
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