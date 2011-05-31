using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using TweetHarbor.Data;
using System.Web.Mvc;
using TweetHarbor.Messaging;

namespace TweetHarbor.Tests.Helpers
{
    public class TestNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ITweetHarborDbContext>().To<TestTweetHarborDbContext>();
            //Binding injection for SnipCmsAuthorize -> SQL Repository
            Bind<IActionInvoker>().To<NinjectActionInvoker>().InSingletonScope();
            Bind<ITweetHarborTwitterService>().To<TestTweetHarborTwitterService>();

        }
    }
}
