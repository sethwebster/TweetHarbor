using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetHarbor.Models
{
    public interface IApplicationImporter
    {
        bool AuthenticateAs(string Username, string Password);
        ICollection<Project> GetProjects();
        bool SetProjectServiceHook(string ProjectUrl, string ServiceHookUrl);
        bool DeleteProjectServiceHook(string ProjectUrl, string ServiceHookUrl);
        bool SetAllProjectServiceHooks(string ServiceHookUrl);
        bool DeleteAllServiceHooks();
    }
}
