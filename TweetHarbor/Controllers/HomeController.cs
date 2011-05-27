using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TweetHarbor.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to TweetHarbor!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
