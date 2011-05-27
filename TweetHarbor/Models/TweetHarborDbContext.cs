using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using TweetHarbor.Data;

namespace TweetHarbor.Models
{
    public class TweetHarborDbContext : DbContext, ITweetHarborDbContext
    {
        public TweetHarborDbContext(){}
        public IDbSet<User> Users { get; set; }
        public IDbSet<Project> Projects { get; set; }
        public void Dispose()
        {
            base.Dispose();
        }
    }
}