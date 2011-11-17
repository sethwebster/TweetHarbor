using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using TweetHarbor.Data;
using TweetHarbor.Models;

namespace TweetHarbor.Security
{
    public class TweetHarborRolesProvider : RoleProvider
    {
        private ITweetHarborDbContext database;

        public TweetHarborRolesProvider() : this(new TweetHarborDbContext())
        {

        }

        public TweetHarborRolesProvider(ITweetHarborDbContext database)
        {
            this.database = database;
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            var user = database.Users.Where(u => u.UserName == username).FirstOrDefault();
            if (null != user && user.IsAdmin)
            {
                return new string[] { "Admin" };
            }
            return new string[] { };
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var user = database.Users.Where(u => u.UserName == username).FirstOrDefault();
            if (null != user && roleName.ToLower() == "admin")
            {
                return user.IsAdmin;
            }
            return false;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}