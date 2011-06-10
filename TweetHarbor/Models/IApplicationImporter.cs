using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetHarbor.Models
{
    public interface IApplicationImporter
    {
        ICollection<Project> GetProjects(string Username, string Password, User user);
    }
}
