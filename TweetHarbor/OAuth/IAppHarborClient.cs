using System;
using TweetHarbor.Models;
namespace TweetHarbor.OAuth
{
    public interface IAppHarborClient
    {
        string GetAccessToken(string Code);
        System.Net.HttpWebRequest GetAuthenticatedWebRequest(string token, string url);
        Uri GetAuthorizationUrl();
        Uri GetAuthorizationUrl(string RedirectUri);
        AppHarborUser GetUserInformation(string token);
        System.Collections.Generic.IEnumerable<TweetHarbor.Models.Project> GetUserProjects(string token);
        System.Web.Mvc.RedirectResult RedirectToAuthorizationResult();
        System.Web.Mvc.RedirectResult RedirectToAuthorizationResult(string redirectUri);
        void SetServiceHookUrl(string token, string projectName, string projectId, string serviceHookUrl);
    }
}
