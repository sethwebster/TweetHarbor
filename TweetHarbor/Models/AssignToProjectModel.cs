using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TweetHarbor.Models
{
    public class AssignToProjectModel
    {
        public Project Project { get; set; }
        public IEnumerable<UserAuthenticationAccount> Accounts { get; set; }

    }
}