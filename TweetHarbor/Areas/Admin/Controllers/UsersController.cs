﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetHarbor.Data;

namespace TweetHarbor.Areas.Admin.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        ITweetHarborDbContext database = null;

        public UsersController(ITweetHarborDbContext database)
        {
            this.database = database;
        }


        public ActionResult Index()
        {
            if (null != HttpContext)
            {
                var user = database.Users.FirstOrDefault(usr => usr.TwitterUserName == HttpContext.User.Identity.Name);
                if (user.IsAdmin)
                {
                    var users = database.Users.OrderBy(u => u.TwitterUserName);
                    return View(users);
                }
                else
                {
                    //TODO: Create better authorization strategy
                    return new RedirectResult("/?f=notauthorized");
                }
            }
            return new EmptyResult();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            database.Dispose();
        }

    }
}