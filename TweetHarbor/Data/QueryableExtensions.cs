using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Data.Entity.Infrastructure;

namespace System.Data
{
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