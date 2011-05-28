using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace TweetHarbor.Tests.Helpers
{
    public class TestableDbSet<T> : IDbSet<T> where T : class, new()
    {
        internal List<T> data = new List<T>();
        public T Add(T entity)
        {
            data.Add(entity);
            return entity;
        }

        public T Attach(T entity)
        {
            // do nothing
            return entity;
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            throw new NotImplementedException();
        }

        public T Create()
        {
            //throw new NotImplementedException();
            var rc = new T();
            return rc;
        }

        public T Find(params object[] keyValues)
        {
            throw new NotImplementedException("Not implemented");
        }

        public System.Collections.ObjectModel.ObservableCollection<T> Local
        {
            get { return new System.Collections.ObjectModel.ObservableCollection<T>(data); }
        }

        public T Remove(T entity)
        {
            data.Remove(entity);
            return entity;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return data.GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }


        public IQueryProvider Provider
        {
            get { return this.data.AsQueryable().Provider; }
        }


        public System.Linq.Expressions.Expression Expression
        {
            get { return data.AsQueryable().Expression; }
        }
    }

}
