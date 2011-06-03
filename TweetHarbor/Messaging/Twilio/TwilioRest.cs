using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Net;
using System.Text;
using System.IO;

namespace TweetHarbor.Messaging.Twilio
{
    public class Account
    {
        const string TWILIO_API_URL = "https://api.twilio.com";

        private string id;
        private string token;

        public Account(string id, string token)
        {
            this.id = id;
            this.token = token;
        }

        private string _download(string uri, Hashtable vars)
        {
            // 1. format query string
            if (vars != null)
            {
                string query = "";
                foreach (DictionaryEntry d in vars)
                    query += "&" + d.Key.ToString() + "=" +
                        HttpUtility.UrlEncode(d.Value.ToString());
                if (query.Length > 0)
                    uri = uri + "?" + query.Substring(1);
            }

            // 2. setup basic authenication
            string authstring = Convert.ToBase64String(
                Encoding.ASCII.GetBytes(String.Format("{0}:{1}",
                this.id, this.token)));

            // 3. perform GET using WebClient
            WebClient client = new WebClient();
            client.Headers.Add("Authorization",
                String.Format("Basic {0}", authstring));
            byte[] resp = client.DownloadData(uri);

            return Encoding.ASCII.GetString(resp);
        }

        private string _upload(string uri, string method, Hashtable vars)
        {
            // 1. format body data
            string data = "";
            if (vars != null)
            {
                foreach (DictionaryEntry d in vars)
                {
                    data += d.Key.ToString() + "=" +
                        HttpUtility.UrlEncode(d.Value.ToString()) + "&";
                }

            }

            // 2. setup basic authenication
            string authstring = Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format("{0}:{1}",
                                                       this.id, this.token)));

            // 3. perform POST/PUT/DELETE using WebClient
            ServicePointManager.Expect100Continue = false;
            Byte[] postbytes = Encoding.ASCII.GetBytes(data);
            WebClient client = new WebClient();

            client.Headers.Add("Authorization",
                               String.Format("Basic {0}", authstring));
            client.Headers.Add("Content-Type",
                               "application/x-www-form-urlencoded");

            byte[] resp = client.UploadData(uri, method, postbytes);

            return Encoding.ASCII.GetString(resp);
        }

        private string _build_uri(string path)
        {
            if (path[0] == '/')
                return TWILIO_API_URL + path;
            else
                return TWILIO_API_URL + "/" + path;
        }

        public string request(string path, string method, Hashtable vars)
        {
            string response = null;

            if (path == null || path.Length <= 0)
                throw (new ArgumentException("Invalid path parameter"));

            method = method.ToUpper();
            if (method == null || (method != "GET" && method != "POST" &&
                                   method != "PUT" && method != "DELETE"))
            {
                throw (new ArgumentException("Invalid method parameter"));
            }

            if (method != "GET" && vars.Count <= 0)
            {
                throw (new ArgumentException("No vars parameters"));
            }

            string url = _build_uri(path);
            try
            {
                if (method == "GET")
                {
                    response = _download(url, vars);
                }
                else
                {
                    response = _upload(url, method, vars);
                }
            }
            catch (WebException e)
            {
                string message = e.Message;

                switch (e.Status)
                {
                    case WebExceptionStatus.TrustFailure:
                        message = "You do not trust the people who " +
                            "issued the certificate being used by twiliorest.dll.";
                        break;
                }

                string var_list = "";
                foreach (DictionaryEntry d in vars)
                {
                    var_list += "&" + d.Key.ToString() + "=" +
                        HttpUtility.UrlEncode(d.Value.ToString());
                }


                string response_str = "";
                using (StreamReader sr = new StreamReader(e.Response.GetResponseStream()))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        response_str += line;
                    }
                }


                message = String.Format("TwilioRestException occurred in the request you sent: \n{0}\n\tURLL: {1}\n\tMETHOD:{2}\n\tVARS:{3}\n\tRESPONSE:{4}",
                                        message,
                                        url,
                                        method,
                                        var_list,
                                        response_str);

                throw new TwilioRestException(message, e);
            }

            return response;
        }

        public string request(string path, string method)
        {
            return this.request(path, method, null);
        }
    }


    [Serializable()]
    public class TwilioRestException : System.Exception
    {

        public TwilioRestException()
            : base()
        {

        }

        public TwilioRestException(string message)
            : base(message)
        {

        }

        public TwilioRestException(string message,
                                   System.Exception innerException)
            : base(message, innerException)
        {

        }

        protected TwilioRestException(System.Runtime.Serialization.SerializationInfo info,
                                      System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {

        }

    }
}