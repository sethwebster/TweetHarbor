using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TweetHarbor;
using TweetHarbor.Controllers;
using TweetHarbor.Models;

namespace TweetHarbor.Tests.Controllers
{
    [TestClass]
    public class ProjectsControllerTest
    {
        [TestMethod]
        public void TestApplicationImport()
        {
            ApplicationImporter a = new ApplicationImporter();
            a.GetProjects("sethwebster@gmail.com", "Mi55ion!", new User());
        }
    }
}
