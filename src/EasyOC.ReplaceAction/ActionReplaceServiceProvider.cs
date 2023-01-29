using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace EasyOC.ReplaceAction
{
    public class ActionReplaceServiceProvider : IActionInvokerProvider
    {
        private readonly ActionReplaceOption _replaceOptions;

        public ActionReplaceServiceProvider(IOptions<ActionReplaceOption> replaceOptions)
        {
            _replaceOptions = replaceOptions.Value;
        }

        public int Order => 10;

        public void OnProvidersExecuted(ActionInvokerProviderContext context)
        {

        }

        public void OnProvidersExecuting(ActionInvokerProviderContext context)
        {
            var actionDescriptor = context.ActionContext.ActionDescriptor as ControllerActionDescriptor;
            var replaceConfg = _replaceOptions.FindOption(actionDescriptor.MethodInfo);
            if (replaceConfg != null)
            {
                actionDescriptor.MethodInfo = replaceConfg.ActionMapping[actionDescriptor.MethodInfo.Name];
                actionDescriptor.ControllerTypeInfo = replaceConfg.NewController.GetTypeInfo();
            }
        }
    }

    public partial class MyRouteBuilder : RouteBuilder
    {
        private readonly ActionReplaceOption _replaceOptions;

        public MyRouteBuilder(IOptions<ActionReplaceOption> replaceOptions, IApplicationBuilder applicationBuilder) : base(applicationBuilder)
        {
            _replaceOptions = replaceOptions.Value;
        }

        public IRouter Build()
        {
            return base.Build();
        }


    }
}
