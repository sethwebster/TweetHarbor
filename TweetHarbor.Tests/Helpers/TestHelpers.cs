using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Ninject;
using Ninject.Web.Mvc;
using Ninject.Modules;

namespace TweetHarbor.Tests.Helpers
{
    public static class TestHelpers
    {
        public static IKernel SetupDependencyInjection()
        {
            var modules = SetupModule();
            IKernel kernel = SetupKernel(modules);
            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
            FilterProviders.Providers.Remove<FilterAttributeFilterProvider>();
            FilterProviders.Providers.Add(new InjectableFilterProvider(kernel));
            return kernel;
        }

        private static IKernel SetupKernel(INinjectModule[] modules)
        {
            return new StandardKernel(modules);
        }

        private static INinjectModule[] SetupModule()
        {
            var modules = new INinjectModule[] {
               new TestNinjectModule()
             };
            return modules;
        }
    }

    public static class ProviderExt
    {
        public static void Remove<TProvider>(this FilterProviderCollection providers) where TProvider : IFilterProvider
        {
            var provider = FilterProviders.Providers.FirstOrDefault(f => f is TProvider);
            if (null != provider)
                providers.Remove(provider);
        }
    }
}
