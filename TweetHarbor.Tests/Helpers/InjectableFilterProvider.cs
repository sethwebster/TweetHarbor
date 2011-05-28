using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using System.Web.Mvc;

namespace TweetHarbor.Tests.Helpers
{
    public class InjectableFilterProvider : FilterAttributeFilterProvider
    {
        public IKernel Kernel { get; set; }

        public InjectableFilterProvider(IKernel kernel)
        {
            Kernel = kernel;
        }

        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor);

            foreach (var filter in filters)
            {
                Kernel.Inject(filter.Instance);
            }

            return filters;
        }
    }
}
