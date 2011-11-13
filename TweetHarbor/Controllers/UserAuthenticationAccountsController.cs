using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;
using Newtonsoft.Json;
using TweetHarbor.Models;

namespace TweetHarbor.Controllers
{
    [Authorize]
    public class UserAuthenticationAccountsController : Controller
    {

        public UserAuthenticationAccountsController(ITweetHarborDbContext database)
        {
            this.database = database;
        }

        private ITweetHarborDbContext database;

        public ContentResult Index(string Type)
        {
            Response.ContentType = "application/json";
            var user = database.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (string.IsNullOrEmpty(Type))
            {
                var ret = from ac in user.AuthenticationAccounts
                          select new
                          {
                              AccountProvider = ac.AccountProvider,
                              ProfilePicUrl = ac.ProfilePicUrl,
                              UserName = ac.UserName
                          };
                return Content(JsonConvert.SerializeObject(ret));
            }
            else
            {
                var ret = from ac in user.AuthenticationAccounts
                          where ac.AccountProvider.ToLower() == Type.ToLower()
                          select new
                          {
                              AccountProvider = ac.AccountProvider,
                              ProfilePicUrl = ac.ProfilePicUrl,
                              UserName = ac.UserName
                          };
                return Content(JsonConvert.SerializeObject(ret));
            }
        }

        public ActionResult AssignToProject(int Id, string Type)
        {
            AssignToProjectModel model = new AssignToProjectModel();
            model.Project = database.Projects.FirstOrDefault(p => p.ProjectId == Id);
            var user = database.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (string.IsNullOrEmpty(Type))
            {
                var ret = from ac in user.AuthenticationAccounts
                          select ac;
                model.Accounts = ret.ToArray();
            }
            else
            {
                var ret = from ac in user.AuthenticationAccounts
                          where ac.AccountProvider.ToLower() == Type.ToLower()
                          select ac;
                model.Accounts = ret.ToArray();
            }
            if (null != model.Project)
            {
                return View(model);
            }
            return Content("Invalid Project");
        }

        [HttpPost]
        public ActionResult AssignToProject(int Id, string Type, int twitteraccount)
        {
            var project = database.Projects.FirstOrDefault(p => p.ProjectId == Id && p.User.UserName == User.Identity.Name);
            if (null != project)
            {
                var uac = database.UserAuthenticationAccounts.FirstOrDefault(uc => uc.UserAuthenticationAccountId == twitteraccount && uc.User.UserName == User.Identity.Name);
                if (null != uac)
                {
                    project.TwitterAccounts.Clear();
                    project.TwitterAccounts.Add(uac);
                    database.SaveChanges();
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    throw new Exception("User Authentication Account not found");
                }
            }
            else
            {
                //TODO: handle this in a more friendly way
                throw new Exception("Project not found");
            }
        }
    }
}
