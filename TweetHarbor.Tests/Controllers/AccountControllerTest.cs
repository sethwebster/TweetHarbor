using TweetHarbor.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using TweetHarbor.Data;
using TweetHarbor.Messaging;
using System.Web.Mvc;
using TweetHarbor.Tests.Helpers;
using System.Security.Principal;
using TweetHarbor.Models;
using System.Web.Security;
using Moq;
using TweetSharp;

namespace TweetHarbor.Tests.Controllers
{


    /// <summary>
    ///This is a test class for AccountControllerTest and is intended
    ///to contain all AccountControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AccountControllerTest
    {


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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void IndexTest()
        {
            //TODO: Pull in action invoker expector
            //ITweetHarborDbContext database = new TestTweetHarborDbContext();
            //ITweetHarborTwitterService twitter = new TestTweetHarborTwitterService();
            //AccountController target = new AccountController(database, twitter); // TODO: Initialize to an appropriate value

            //ControllerActionInvoker c = new ControllerActionInvoker();
            //target.SetFakeControllerContext();

            //c.InvokeAction(target.ControllerContext, "Index");



            //ActionResult expected = null; // TODO: Initialize to an appropriate value
            //ActionResult actual;
            //actual = target.Index();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        public void GlobalNotificationToggle_SendPrivateTweet_True_Correct()
        {

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
            var ts = new TestTweetHarborTwitterService();

            var auth = new Mock<IFormsAuthenticationWrapper>();

            var controller = new AccountController(db, ts, auth.Object);

            var ident = new GenericIdentity("localtestuser");
            System.Security.Principal.GenericPrincipal c = new System.Security.Principal.GenericPrincipal(ident, new string[] { });

            controller.SetFakeControllerContext(c);

            var res = controller.GlobalNotificationToggle("SendPrivateTweet", true);
            Assert.IsInstanceOfType(res.Data, typeof(JsonResultModel));
            var rm = (JsonResultModel)res.Data;
            Assert.IsTrue(rm.Success);
            Assert.IsTrue(rm.Message == "Value has been updated");

            Assert.AreEqual(true, db.Users.FirstOrDefault(u => u.UserName== ident.Name).SendPrivateTweet);

        }
        [TestMethod]
        public void GlobalNotificationToggle_SendPublicTweet_True_Correct()
        {

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
            var ts = new TestTweetHarborTwitterService();


            var auth = new Mock<IFormsAuthenticationWrapper>();

            var controller = new AccountController(db, ts, auth.Object);

            var ident = new GenericIdentity("localtestuser");
            System.Security.Principal.GenericPrincipal c = new System.Security.Principal.GenericPrincipal(ident, new string[] { });

            controller.SetFakeControllerContext(c);

            var res = controller.GlobalNotificationToggle("SendPublicTweet", true);
            Assert.IsInstanceOfType(res.Data, typeof(JsonResultModel));
            var rm = (JsonResultModel)res.Data;
            Assert.IsTrue(rm.Success);
            Assert.IsTrue(rm.Message == "Value has been updated");

            Assert.AreEqual(true, db.Users.FirstOrDefault(u => u.UserName == ident.Name).SendPublicTweet);

        }
        [TestMethod]
        public void GlobalNotificationToggle_SendPRivateTweet_False_Correct()
        {

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
            var ts = new TestTweetHarborTwitterService();

            var auth = new Mock<IFormsAuthenticationWrapper>();

            var controller = new AccountController(db, ts, auth.Object);

            var ident = new GenericIdentity("localtestuser");
            System.Security.Principal.GenericPrincipal c = new System.Security.Principal.GenericPrincipal(ident, new string[] { });

            controller.SetFakeControllerContext(c);

            var res = controller.GlobalNotificationToggle("SendPrivateTweet", true);
            Assert.IsInstanceOfType(res.Data, typeof(JsonResultModel));
            var rm = (JsonResultModel)res.Data;
            Assert.IsTrue(rm.Success);
            Assert.IsTrue(rm.Message == "Value has been updated");

            Assert.AreEqual(true, db.Users.FirstOrDefault(u => u.UserName == ident.Name).SendPrivateTweet);

        }
        [TestMethod]
        public void GlobalNotificationToggle_SendPublicTweet_False_Correct()
        {

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
            var ts = new TestTweetHarborTwitterService();

            var auth = new Mock<IFormsAuthenticationWrapper>();

            var controller = new AccountController(db, ts, auth.Object);

            var ident = new GenericIdentity("localtestuser");
            System.Security.Principal.GenericPrincipal c = new System.Security.Principal.GenericPrincipal(ident, new string[] { });

            controller.SetFakeControllerContext(c);

            var res = controller.GlobalNotificationToggle("SendPublicTweet", true);
            Assert.IsInstanceOfType(res.Data, typeof(JsonResultModel));
            var rm = (JsonResultModel)res.Data;
            Assert.IsTrue(rm.Success);
            Assert.IsTrue(rm.Message == "Value has been updated");

            Assert.AreEqual(true, db.Users.FirstOrDefault(u => u.UserName== ident.Name).SendPublicTweet);

        }
        [TestMethod]
        public void GlobalNotificationToggle_SendPublicTweet_False_UserDoesNotExist()
        {

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
            var ts = new TestTweetHarborTwitterService();

            var auth = new Mock<IFormsAuthenticationWrapper>();

            var controller = new AccountController(db, ts, auth.Object);

            var ident = new GenericIdentity("localtestusermissing");
            System.Security.Principal.GenericPrincipal c = new System.Security.Principal.GenericPrincipal(ident, new string[] { });

            controller.SetFakeControllerContext(c);

            var res = controller.GlobalNotificationToggle("SendPublicTweet", true);
            Assert.IsInstanceOfType(res.Data, typeof(JsonResultModel));
            var rm = (JsonResultModel)res.Data;
            Assert.IsFalse(rm.Success);
            Assert.IsTrue(rm.Error == "User Not Found");
        }
        [TestMethod]
        public void GlobalNotificationToggle_SendPrivateTweet_False_UserDoesNotExist()
        {

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
            var ts = new TestTweetHarborTwitterService();


            var auth = new Mock<IFormsAuthenticationWrapper>();

            var controller = new AccountController(db, ts, auth.Object);

            var ident = new GenericIdentity("localtestusermissing");
            System.Security.Principal.GenericPrincipal c = new System.Security.Principal.GenericPrincipal(ident, new string[] { });

            controller.SetFakeControllerContext(c);

            var res = controller.GlobalNotificationToggle("SendPrivateTweet", true);
            Assert.IsInstanceOfType(res.Data, typeof(JsonResultModel));
            var rm = (JsonResultModel)res.Data;
            Assert.IsFalse(rm.Success);
            Assert.IsTrue(rm.Error == "User Not Found");
        }

        [TestMethod]
        public void AuthorizeCallback_NewUser()
        {
            var db = new TestTweetHarborDbContext();

            var ts = new Mock<ITweetHarborTwitterService>();
            string token = Guid.NewGuid().ToString();
            string verifier = Guid.NewGuid().ToString();
            string TestUsername = "LocalTestUser";

            var user = UserHelper.ArrangeNewUserDefault();
           

            ts.Setup<OAuthAccessToken>(a => a.GetAccessToken(It.IsAny<OAuthRequestToken>(), It.IsAny<string>())).Returns(new OAuthAccessToken() { Token = token, TokenSecret = verifier });
            ts.Setup<TwitterUser>(a => a.VerifyCredentials()).Returns(new TwitterUser() { ScreenName = TestUsername });

            var auth = new Mock<IFormsAuthenticationWrapper>();

            var controller = new AccountController(db, ts.Object, auth.Object);
            controller.SetFakeControllerContext();
            var val = controller.AuthorizeCallback(null, token, verifier, null);

            Assert.AreNotEqual(0, db.Users.Count());
            Assert.AreEqual(token, db.Users.First().AuthenticationAccounts.FirstOrDefault(ac=>ac.AccountProvider=="twitter").OAuthToken);
            Assert.AreEqual(verifier, db.Users.First().AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "twitter").OAuthTokenSecret);
            Assert.AreEqual(TestUsername, db.Users.First().AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "twitter").UserName);

        }

        [TestMethod]
        public void AuthorizeCallback_ExistingUser()
        {
            var db = new TestTweetHarborDbContext();
            var ts = new Mock<ITweetHarborTwitterService>();
            string token = Guid.NewGuid().ToString();
            string verifier = Guid.NewGuid().ToString();
            string TestUsername = "twitteruser";
            string imageUrl = "http://test.com/image.jpg";

            var user = UserHelper.ArrangeNewUserDefault();
            db.Users.Add(user);

            ts.Setup<OAuthAccessToken>(a => a.GetAccessToken(It.IsAny<OAuthRequestToken>(), It.IsAny<string>())).Returns(new OAuthAccessToken() { Token = token, TokenSecret = verifier });
            ts.Setup<TwitterUser>(a => a.VerifyCredentials()).Returns(new TwitterUser() { ScreenName = TestUsername, ProfileImageUrl = imageUrl });

            var auth = new Mock<IFormsAuthenticationWrapper>();

            var controller = new AccountController(db, ts.Object, auth.Object);
            controller.SetFakeControllerContext();
            var val = controller.AuthorizeCallback(null, token, verifier, null);

            Assert.AreEqual(1, db.Users.Count());
            Assert.AreEqual(token, db.Users.First().AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "twitter").OAuthToken);
            Assert.AreEqual(verifier, db.Users.First().AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "twitter").OAuthTokenSecret);
            Assert.AreEqual(TestUsername, db.Users.First().AuthenticationAccounts.FirstOrDefault(ac => ac.AccountProvider == "twitter").UserName);
            Assert.AreEqual(imageUrl, db.Users.First().UserProfilePicUrl);

        }

    }
}
