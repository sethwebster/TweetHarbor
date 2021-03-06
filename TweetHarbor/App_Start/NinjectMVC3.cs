[assembly: WebActivator.PreApplicationStartMethod(typeof(TweetHarbor.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(TweetHarbor.App_Start.NinjectMVC3), "Stop")]

namespace TweetHarbor.App_Start
{
    using System.Reflection;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Mvc;
    using TweetHarbor.Data;
    using TweetHarbor.Models;
    using TweetSharp;
    using TweetHarbor.Messaging;
    using TweetHarbor.OAuth;
    using System.Configuration;

    public static class NinjectMVC3
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestModule));
            DynamicModuleUtility.RegisterModule(typeof(HttpApplicationInitializationModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            MvcApplication.SharedKernel = kernel;
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ITweetHarborDbContext>().To<TweetHarborDbContext>();
            kernel.Bind<ITweetHarborTextMessageService>().To<TweetHarborTextMessageService>();
            kernel.Bind<IFormsAuthenticationWrapper>().To<TweetHarborFormsAuthentication>();
            var clientId = ConfigurationManager.AppSettings["AppHarborOAuthClientId"];
            var secret = ConfigurationManager.AppSettings["AppHarborOAuthSecret"];

            kernel.Bind<AppHarbor.Client.IAppHarborClient>().To<AppHarbor.Client.AppHarborClient>().WithConstructorArgument("clientId", clientId).WithConstructorArgument("secret", secret);
#if DEBUG
            kernel.Bind<ITweetHarborTwitterService>().To<TweetHarborTwitterServiceOffline>().WithConstructorArgument("ConsumerKey", TwitterHelper.ConsumerKey).WithConstructorArgument("ConsumerSecret", TwitterHelper.ConsumerSecret);
#else
            kernel.Bind<ITweetHarborTwitterService>().To<TweetHarborTwitterService>().WithConstructorArgument("ConsumerKey", TwitterHelper.ConsumerKey).WithConstructorArgument("ConsumerSecret", TwitterHelper.ConsumerSecret);

#endif
        }
    }
}
