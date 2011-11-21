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
        /// <summary>
        /// Method used for testing actual web call -- not to be used when in actual test mode (breaks)
        /// </summary>
#if !TEST
        //  [TestMethod]
        public void TestRemote()
        {
            string testStr = "{\"application\": { \"name\": \"Test Project 1\" },   \"build\": {    \"commit\": {      \"id\": \"" + Guid.NewGuid() + "\", \"message\": \"Implement foo\"  }, \"status\": \"succeeded\" } }";

            WebClient wc = new WebClient();
            wc.Headers.Add("Content-Type", "application/json");
            var data = wc.UploadData("http://localhost:9090/notify/new/sethwebster?token=3fcb79e66ddd994209ffb22f61618304", "POST", Encoding.ASCII.GetBytes(testStr));

            var str = Encoding.ASCII.GetString(data);
        }
#endif

        [TestMethod]
        public void TestBuildSuccess()
        {
            string testStr = "{\"application\": { \"name\": \"Test Project 1\" },   \"build\": {    \"commit\": {      \"id\": \"" + Guid.NewGuid() + "\", \"message\": \"Implement foo\"  }, \"status\": \"succeeded\" } }";
            var o = JsonConvert.DeserializeObject<Notification>(testStr);

            var db = new TestTweetHarborDbContext();
            var user = UserHelper.ArrangeNewUserDefault();
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

            var res = controller.New(user.UserName, user.UniqueId, o);

            Assert.IsInstanceOfType(res, typeof(JsonResult));
            Assert.IsInstanceOfType((res as JsonResult).Data, typeof(JsonResultModel));

            Assert.AreEqual(true, ((res as JsonResult).Data as JsonResultModel).Success);
        }

        [TestMethod]
        public void TestBuildSuccess_MutedMessage()
        {
            string testStr = "{\"application\": { \"name\": \"Test Project 1\" },   \"build\": {    \"commit\": {      \"id\": \"" + Guid.NewGuid() + "\", \"message\": \"Implement foo-\"  }, \"status\": \"succeeded\" } }";
            var deserializedJsonObject = JsonConvert.DeserializeObject<Notification>(testStr);

            var db = new TestTweetHarborDbContext();
            var user = UserHelper.ArrangeNewUserDefault();
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

            var res = controller.New(user.UserName, user.UniqueId, deserializedJsonObject);

            proj = user.Projects.FirstOrDefault(p => p.ProjectName == deserializedJsonObject.application.name);

            Assert.IsInstanceOfType(res, typeof(JsonResult));
            Assert.IsInstanceOfType((res as JsonResult).Data, typeof(JsonResultModel));

            Assert.AreNotEqual(0, proj.ProjectNotifications.Count());
            Assert.AreEqual(0, proj.OutboundNotifications.Count());

            Assert.AreEqual(true, ((res as JsonResult).Data as JsonResultModel).Success);
        }

        [TestMethod]
        public void TestBuildFailure()
        {
            string testStr = "{\"application\": { \"name\": \"Test Project 1\" },   \"build\": {    \"commit\": {      \"id\": \"" + Guid.NewGuid() + "\", \"message\": \"Implement foo\"  }, \"status\": \"succeeded\" } }";
            var o = JsonConvert.DeserializeObject<Notification>(testStr);

            var db = new TestTweetHarborDbContext();
            var user = UserHelper.ArrangeNewUserDefault();
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

            var res = controller.New(user.UserName, user.UniqueId, o);

            Assert.IsInstanceOfType(res, typeof(JsonResult));
            Assert.IsInstanceOfType((res as JsonResult).Data, typeof(JsonResultModel));

            Assert.AreEqual(true, ((res as JsonResult).Data as JsonResultModel).Success);
        }

        [TestMethod]
        public void TestSendSmsMessages_OutboundNotificationCreated()
        {
            string testStr = "{\"application\": { \"name\": \"Test Project 1\" },   \"build\": {    \"commit\": {      \"id\": \"" + Guid.NewGuid() + "\", \"message\": \"Implement foo\"  }, \"status\": \"succeeded\" } }";
            var o = JsonConvert.DeserializeObject<Notification>(testStr);

            TestTweetHarborDbContext db = new TestTweetHarborDbContext();

            var m = new Mock<ITweetHarborTextMessageService>();
            m.Setup(a => a.SendText("", ""));

            var user = UserHelper.ArrangeNewUserDefault();
            db.Users.Add(user);

            var proj = new Project()
                {
                    ProjectName = o.application.name,
                    SendPrivateTweetOnFailure = true,
                    SendPrivateTweetOnSuccess = true,
                    SendPublicTweetOnFailure = false,
                    SendPublicTweetOnSuccess = true,
                    SendTextOnSuccess = true,
                    SendTextOnFailure = false,
                    User = user
                };
            user.Projects.Add(proj);
            db.Projects.Add(proj);
            var tmr = new TextMessageRecipient()
            {
                Name = "App Test",
                PhoneNumber = "5201235678",
            };
            proj.TextMessageRecipients.Add(tmr);

            var dmr = new TwitterMessageRecipient()
            {
                ScreenName = "testuser",
            };

            proj.MessageRecipients.Add(dmr);

            var controller = new NotifyController(db, new TestTweetHarborTwitterService(), m.Object);

            var res = controller.New(user.UserName, user.UniqueId, o);

            Assert.IsInstanceOfType(res, typeof(JsonResult));
            Assert.IsInstanceOfType((res as JsonResult).Data, typeof(JsonResultModel));

            Assert.AreEqual(true, ((res as JsonResult).Data as JsonResultModel).Success);

            Assert.AreNotEqual(0, proj.OutboundNotifications.Count);
            Assert.AreNotEqual(0, proj.OutboundNotifications.First().Message.Length);
            Assert.AreEqual(2, proj.OutboundNotifications.Count);

            var nots = proj.OutboundNotifications.OrderBy(pj => pj.NotificationType);

            Assert.AreEqual("SMS", nots.First().NotificationType);
            Assert.AreEqual("5201235678", nots.First().Recipient);

            Assert.AreEqual("Twitter", nots.ElementAt(1).NotificationType);
            Assert.AreEqual(dmr.ScreenName, nots.ElementAt(1).Recipient);
        }

        [TestMethod]
        public void TestSendSmsMessages_InboundNotificationCreated()
        {
            string testStr = "{\"application\": { \"name\": \"Test Project 1\" },   \"build\": {    \"commit\": {      \"id\": \"" + Guid.NewGuid() + "\", \"message\": \"Implement foo\"  }, \"status\": \"succeeded\" } }";
            var o = JsonConvert.DeserializeObject<Notification>(testStr);

            TestTweetHarborDbContext db = new TestTweetHarborDbContext();

            var m = new Mock<ITweetHarborTextMessageService>();
            m.Setup(a => a.SendText("", ""));

            var user = UserHelper.ArrangeNewUserDefault();
            db.Users.Add(user);

            var proj = new Project()
            {
                ProjectName = o.application.name,
                SendPrivateTweetOnFailure = true,
                SendPrivateTweetOnSuccess = true,
                SendPublicTweetOnFailure = false,
                SendPublicTweetOnSuccess = true,
                SendTextOnSuccess = true,
                SendTextOnFailure = false,
                User = user
            };
            user.Projects.Add(proj);
            db.Projects.Add(proj);
            var tmr = new TextMessageRecipient()
            {
                Name = "App Test",
                PhoneNumber = "5201235678",
            };
            proj.TextMessageRecipients.Add(tmr);

            var controller = new NotifyController(db, new TestTweetHarborTwitterService(), m.Object);

            var res = controller.New(user.UserName, user.UniqueId, o);

            Assert.IsInstanceOfType(res, typeof(JsonResult));
            Assert.IsInstanceOfType((res as JsonResult).Data, typeof(JsonResultModel));

            Assert.AreEqual(true, ((res as JsonResult).Data as JsonResultModel).Success);

            Assert.AreNotEqual(0, proj.ProjectNotifications.Count);
            Assert.AreNotEqual(0, proj.ProjectNotifications.First().Build.commit.message.Length);

        }

        [TestMethod]
        public void TestDeTokenizeString()
        {
            string Input = "{application:name} is being tested on @TweetHarbor 'cause it rocks the {build:commit:id} magic {build:commit:message}";
            Notification n = new Notification()
            {
                application = new Application()
                {
                    name = "TestApp",
                },
                build = new Build()
                {
                    BuildId = 10,
                    commit = new Commit()
                    {
                        id = Guid.NewGuid().ToString(),
                        message = "Some testin' goin' on"
                    },
                    status = "succeeded"
                }
            };

            Project p = new Project()
            {
                DateCreated = DateTime.Now.AddDays(-1),
                ProjectName = "TestApp"
            };

            var result = NotifyController.DeTokenizeString(Input, p, n);

            Assert.AreEqual("TestApp is being tested on @TweetHarbor 'cause it rocks the " + n.build.commit.id.Substring(0, 7) + " magic Some testin' goin' on", result);

        }

    }
}
