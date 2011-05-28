using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity;
using TweetHarbor.Models;
using Devtalk.EF.CodeFirst;

namespace TweetHarbor
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
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
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
#if DEBUG
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<TweetHarborDbContext>());
#else
            Database.SetInitializer<TweetHarborDbContext>(null);
#endif
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //try
            //{
            //    var exception = Server.GetLastError();
            //    var logEntry = new LogEntry
            //    {
            //        Date = DateTime.Now,
            //        Message = exception.Message,
            //        StackTrace = exception.StackTrace,
            //    };

            //    var datacontext = new LogDBDataContext();
            //    datacontext.LogEntries.InsertOnSubmit(logEntry);
            //    datacontext.SubmitChanges();
            //}
            //catch (Exception)
            //{
            //    // failed to record exception
            //}
        }

    }
}