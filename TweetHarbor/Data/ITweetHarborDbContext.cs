using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using TweetHarbor.Models;

namespace TweetHarbor.Data
{
    public interface ITweetHarborDbContext : IDisposable
    {
        IDbSet<User> Users { get; set; }
        IDbSet<Project> Projects { get; set; }
        int SaveChanges();
    }
}