using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace AppHarbor.Client
{
    public class AppHarborClient : IAppHarborClient
    {
        string clientId = string.Empty;
        string secret = string.Empty;
        public AppHarborClient(string clientId, string secret)
        {
            this.clientId = clientId;
            this.secret = secret;
        }

        public Uri GetAuthorizationUrl()
        {
            return new Uri(string.Format("https://appharbor.com/user/authorizations/new?client_id={0}", clientId));
        }

        public Uri GetAuthorizationUrl(string RedirectUri)
        {
            return new Uri(string.Format("https://appharbor.com/user/authorizations/new?client_id={0}&redirect_uri={1}", clientId, HttpUtility.UrlEncode(RedirectUri)));
        }

        public RedirectResult RedirectToAuthorizationResult(string redirectUri)
        {
            return new RedirectResult(GetAuthorizationUrl(redirectUri).AbsoluteUri);
        }

        public RedirectResult RedirectToAuthorizationResult()
        {
            return new RedirectResult(GetAuthorizationUrl().ToString());
        }

        public string GetAccessToken(string Code)
        {
            WebRequest req = WebRequest.Create("https://appharbor.com/tokens");
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";

            string parameters = string.Format("client_id={0}&client_secret={1}&code={2}", clientId, secret, HttpUtility.UrlEncode(Code));
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parameters);
            req.ContentLength = bytes.Length;
            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();
            System.Net.WebResponse resp = req.GetResponse();
            if (resp == null) return null;
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            var respStr = sr.ReadToEnd().Trim();
            var nvc = HttpUtility.ParseQueryString(respStr);

            return nvc["access_token"];
        }

        public User GetUserInformation(string token)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("Authorization", "BEARER " + token);
            wc.Headers.Add("Accept", "application/json");
            var str = wc.DownloadString("https://appharbor.com/user");
            dynamic obj = JsonConvert.DeserializeObject(str);
            User ret = new User()
            {
                EmailAddress = obj.email_addresses[0].Value,
                UserName = obj.username,
                UniqueId = obj.id
            };
            return ret;
        }

        //TODO: Make the return here not of the TweetHarbor.Project
        public IEnumerable<Project> GetUserProjects(string token)
        {
            WebClient cl = GetAuthenticatedWebClient(token);
            var str = cl.DownloadString("https://appharbor.com/application");

            dynamic obj = JsonConvert.DeserializeObject(str);
            List<Project> ret = new List<Project>();
            foreach (var o in obj)
            {
                ret.Add(new Project()
                    {
                        ProjectName = o.name,
                        ProjectUrl = o.url
                    });
            }

            return ret;
        }

        private static WebClient GetAuthenticatedWebClient(string token)
        {
            WebClient cl = new WebClient();
            cl.Headers.Add("Accept", "application/json");
            cl.Headers.Add("Authorization", "BEARER " + token);

            return cl;
        }

        private IEnumerable<Project> projects;
        public Project GetProject(string token, string projectName)
        {
            if (null == projects)
            {
                projects = GetUserProjects(token);
            }

            return (from p in projects
                    where p.ProjectName == projectName
                    select p).FirstOrDefault();

            //TODO: make this more efficient
        }

        public HttpWebRequest GetAuthenticatedWebRequest(string token, string url)
        {
            HttpWebRequest ret = (HttpWebRequest)HttpWebRequest.Create(url);
            ret.Headers.Add("Authorization", "BEARER " + token);
            return ret;
        }

        bool ServiceHookExists(string token, string projectName, string serviceHookUrl)
        {
            try
            {
                var url = string.Format(GetProject(token, projectName).ProjectUrl + "/servicehook", projectName);
                var cli = GetAuthenticatedWebRequest(token, url);
                cli.Accept = "application/json";
                var sr = new StreamReader(cli.GetResponse().GetResponseStream());

                var resp = sr.ReadToEnd();

                //TODO: Remove this later
                var jsonData = TransformToJSON(resp);

                dynamic obj = JsonConvert.DeserializeObject(jsonData);

                foreach (var o in obj)
                {
                    if (o.ServiceHook == serviceHookUrl)
                        return true;
                }
            }
            catch (WebException we)
            {
                //TODO: What is the "right" way to do this?
                // We will swallow the 404 here because AppHarbor returns
                // a 404 in the case that I am a collaborator on a project
                // and not the owner, and I try to access the servicehook url 
                if (!we.Message.Contains("404"))
                {
                    throw we;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return false;
        }

        public void SetServiceHookUrl(string token, string projectName, string projectId, string serviceHookUrl)
        {
            if (!ServiceHookExists(token, projectName, serviceHookUrl))
            {
                try
                {
                    var url = string.Format(GetProject(token, projectName).ProjectUrl + "/servicehook", projectName);
                    var req = GetAuthenticatedWebRequest(token, url);
                    req.Accept = "application/json";
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.Method = "POST";

                    string parameters = "ServiceHook.Url=" + serviceHookUrl;

                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(parameters);
                    req.ContentLength = bytes.Length;
                    System.IO.Stream os = req.GetRequestStream();
                    os.Write(bytes, 0, bytes.Length); //Push it out there
                    os.Close();
                    System.Net.WebResponse resp = req.GetResponse();

                    System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                    var respStr = sr.ReadToEnd().Trim();
                    var nvc = HttpUtility.ParseQueryString(respStr);

                }
                catch (WebException we)
                {
                    //TODO: What is the "right" way to do this?
                    // We will swallow the 404 here because AppHarbor returns
                    // a 404 in the case that I am a collaborator on a project
                    // and not the owner, and I try to access the servicehook url 
                    if (!we.Message.Contains("404"))
                    {
                        throw we;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// This method should be removed once we have the real JSON api in place
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        private string TransformToJSON(string resp)
        {
            var str = resp.Substring(resp.IndexOf("<h2>Your hooks</h2>") + "<h2>Your hooks</h2>".Length);
            str = str.Substring(0, str.IndexOf("</table>"));
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(str);
            var data = doc.DocumentNode.Descendants("td");
            List<Object> l = new List<Object>();
            foreach (var e in data)
            {
                l.Add(new
                {
                    ServiceHook = e.InnerText
                });
            }
            return JsonConvert.SerializeObject(l);
        }

    }
}