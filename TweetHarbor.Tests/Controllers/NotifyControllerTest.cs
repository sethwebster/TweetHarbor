using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TweetHarbor.Controllers;
using Newtonsoft.Json;
using TweetHarbor.Models;

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

        [TestMethod]
        public void TestNew()
        {
            string testStr = "{\"application\": { \"name\": \"Foo\" },   \"build\": {    \"commit\": {      \"id\": \"77d991fe61187d205f329ddf9387d118a09fadcd\", \"message\": \"Implement foo\"  }, \"status\": \"succeeded\" } }";

            var o = JsonConvert.DeserializeObject<Notification>(testStr);
            var c = new NotifyController(null);
            var res = c.New("", "", o);

        }

        
    }
}
