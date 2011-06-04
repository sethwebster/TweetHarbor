using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TweetHarbor.Controllers;
using Newtonsoft.Json;
using TweetHarbor.Models;
using System.Web.Mvc;
using TweetHarbor.Tests.Helpers;
using System.Collections.ObjectModel;
using TweetHarbor.Messaging;
using System.Net;
using Moq;

namespace TweetHarbor.Tests.Controllers
{
    /// <summary>
    /// Summary description for NotifyControllerTest
    /// </summary>
    [TestClass]
    public class NotifyControllerTest
    {
        public NotifyControllerTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        // 
       // [TestMethod]
        public void TestRemote()
        {
            string testStr = "{\"application\": { \"name\": \"Test Project 1\" },   \"build\": {    \"commit\": {      \"id\": \""+Guid.NewGuid()+"\", \"message\": \"Implement foo\"  }, \"status\": \"succeeded\" } }";

            WebClient wc = new WebClient();
            wc.Headers.Add("Content-Type", "application/json");
            var data = wc.UploadData("http://localhost:9090/notify/new/sethwebster?token=3fcb79e66ddd994209ffb22f61618304", "POST", Encoding.ASCII.GetBytes(testStr));

            var str = Encoding.ASCII.GetString(data);
        }
        [TestMethod]
        public void TestBuildSuccess()
        {
            string testStr = "{\"application\": { \"name\": \"Test Project 1\" },   \"build\": {    \"commit\": {      \"id\": \"" + Guid.NewGuid() + "\", \"message\": \"Implement foo\"  }, \"status\": \"succeeded\" } }";
            var o = JsonConvert.DeserializeObject<Notification>(testStr);

            var db = new TestTweetHarborDbContext();
            var user = new User()
            {
                EmailAddress = "sethwebster@gmail.com",
                OAuthToken = "<FakeOauthToken>",
                OAuthTokenSecret = "<FakeOauthTokenSecret>",
                UniqueId = "db7a3a64156d0b33beae93fe99ca599e",
                SendPrivateTweet = true,
                SendPublicTweet = false,
                TwitterUserName = "sethwebster"
            };
            db.Users.Add(user);

            var proj = new Project()
                {
                    ProjectName = "The Test Project",
                    SendPrivateTweetOnFailure = true,
                    SendPrivateTweetOnSuccess = true,
                    SendPublicTweetOnFailure = false,
                    SendPublicTweetOnSuccess = true,
                    User = user
                };

            db.Projects.Add(proj);

            proj.MessageRecipients.Add(new TwitterMessageRecipient() { ScreenName = "sethwebster" });

            user.Projects = new Collection<Project>();
            user.Projects.Add(proj);

            var m = new Mock<ITweetHarborTextMessageService>();

            m.Setup(a => a.SendText("", ""));

            var controller = new NotifyController(db, new TestTweetHarborTwitterService(), m.Object);
            MvcMockHelpers.SetFakeControllerContext(controller);

            var res = controller.New(user.TwitterUserName, user.UniqueId, o);

            Assert.IsInstanceOfType(res, typeof(JsonResult));
            Assert.IsInstanceOfType((res as JsonResult).Data, typeof(JsonResultModel));

            Assert.AreEqual(true, ((res as JsonResult).Data as JsonResultModel).Success);
        }

        [TestMethod]
        public void TestBuildFailure()
        {
            string testStr = "{\"application\": { \"name\": \"Test Project 1\" },   \"build\": {    \"commit\": {      \"id\": \"" + Guid.NewGuid() + "\", \"message\": \"Implement foo\"  }, \"status\": \"succeeded\" } }";
            var o = JsonConvert.DeserializeObject<Notification>(testStr);

            var db = new TestTweetHarborDbContext();
            var user = new User()
            {
                EmailAddress = "sethwebster@gmail.com",
                OAuthToken = "<FakeOauthToken>",
                OAuthTokenSecret = "<FakeOauthTokenSecret>",
                UniqueId = "db7a3a64156d0b33beae93fe99ca599e",
                SendPrivateTweet = true,
                SendPublicTweet = false,
                TwitterUserName = "sethwebster"
            };
            db.Users.Add(user);

            var proj = new Project()
            {
                ProjectName = "The Test Project",
                SendPrivateTweetOnFailure = true,
                SendPrivateTweetOnSuccess = true,
                SendPublicTweetOnFailure = false,
                SendPublicTweetOnSuccess = true,
                User = user
            };

            db.Projects.Add(proj);

            user.Projects = new Collection<Project>();
            user.Projects.Add(proj);

            var m = new Mock<ITweetHarborTextMessageService>();

            m.Setup(a => a.SendText("", ""));

            var controller = new NotifyController(db, new TestTweetHarborTwitterService(), m.Object); 
            
            MvcMockHelpers.SetFakeControllerContext(controller);

            var res = controller.New(user.TwitterUserName, user.UniqueId, o);

            Assert.IsInstanceOfType(res, typeof(JsonResult));
            Assert.IsInstanceOfType((res as JsonResult).Data, typeof(JsonResultModel));

            Assert.AreEqual(true, ((res as JsonResult).Data as JsonResultModel).Success);
        }

    }
}
