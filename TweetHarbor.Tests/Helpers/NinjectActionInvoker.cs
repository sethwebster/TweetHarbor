using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Ninject;

namespace TweetHarbor.Tests.Helpers
{
    public class NinjectActionInvoker : ControllerActionInvoker
    {
        private readonly IKernel _kernel;

        public NinjectActionInvoker(IKernel kernel)
        {
            _kernel = kernel;
        }

        protected override ActionExecutedContext InvokeActionMethodWithFilters(
            ControllerContext controllerContext,
            IList<IActionFilter> filters,
            ActionDescriptor actionDescriptor,
            IDictionary<string, object> parameters)
        {
            foreach (IActionFilter actionFilter in filters)
            {
                _kernel.Inject(actionFilter);
            }
            return base.InvokeActionMethodWithFilters(
                controllerContext, filters, actionDescriptor, parameters);
        }
    }
}
