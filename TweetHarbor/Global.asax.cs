using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using TweetHarbor.Models;
using Devtalk.EF.CodeFirst;
using TweetHarbor.Data;
using Ninject;

namespace TweetHarbor
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static IKernel SharedKernel { get; set; }
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Account", action = "Index", id = UrlParameter.Optional },
                new string[] { "TweetHarbor.Controllers" }
                // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
#if DEBUG
            Database.SetInitializer(new LocalDataInitializationStrategy());
#else
            Database.SetInitializer<TweetHarborDbContext>(null);
#endif
        }

        protected void Application_Error(object sender, EventArgs e)
        {

#if !DEBUG            
            try
            {
                var exception = Server.GetLastError();
                var logEntry = new LogEntry
                {
                    Date = DateTime.Now,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                };
                //TODO: Get this done with DI
                TweetHarborDbContext d = new TweetHarborDbContext();


            }
            catch (Exception)
            {
                // failed to record exception
            }
#endif

        }

    }
}