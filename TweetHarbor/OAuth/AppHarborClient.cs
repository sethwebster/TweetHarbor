using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using Newtonsoft.Json;
using TweetHarbor.Models;

namespace TweetHarbor.OAuth
{
    public class AppHarborClient
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
            return new Uri(string.Format("https://appharbor.com/user/authorizations/new?client_id={0}&redirect_uri={1}", clientId, RedirectUri));
        }

        public RedirectResult RedirectToAuthorizationResult(string redirectUri)
        {
            return new RedirectResult(GetAuthorizationUrl(redirectUri).ToString());
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

        public AppHarborUser GetUserInformation(string token)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("Authorization", "BEARER " + token);
            wc.Headers.Add("Accept", "application/json");
            var str = wc.DownloadString("https://appharbor.com/user");
            dynamic obj = JsonConvert.DeserializeObject(str);
            AppHarborUser ret = new AppHarborUser()
            {
                EmailAddress = obj.email_addresses[0].Value,
                UserName = obj.username,
                UniqueId = obj.id
            };
            return ret;
        }

        public IEnumerable<Project> GetUserProjects(string token)
        {
            WebClient cl = new WebClient();
            cl.Headers.Add("Accept", "application/json");
            cl.Headers.Add("Authorization", "BEARER " + token);
            var str = cl.DownloadString("https://appharbor.com/application");

            dynamic obj = JsonConvert.DeserializeObject(str);
            List<Project> ret = new List<Project>();
            foreach (var o in obj)
            {
                ret.Add(new Project()
                    {
                        ProjectName = o.name,
                        AppHarborProjectUrl = o.url
                    });
            }

            return ret;

        }
    }
}