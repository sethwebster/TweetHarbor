using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using TweetHarbor.Models;
using System.Data.Entity.Infrastructure;

namespace TweetHarbor.Data
{
    public interface ITweetHarborDbContext : IDisposable
    {
        IDbSet<User> Users { get; set; }
        IDbSet<Project> Projects { get; set; }
        int SaveChanges();
    }

    public static class QueryableExtensions
    {
        public static IQueryable<T> Include<T>
                (this IQueryable<T> sequence, string path)
        {
            var objectQuery = sequence as DbQuery<T>;
            if (objectQuery != null)
            {
                return objectQuery.Include(path);
            }
            return sequence;
        }
    }
}