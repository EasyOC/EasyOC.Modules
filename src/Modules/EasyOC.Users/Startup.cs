using EasyOC.ReplaceAction;
using EasyOC.Users.Controllers;
using EasyOC.Users.Handlers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.OpenId.Controllers;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Handlers;

namespace EasyOC.Users
{
    [Feature("EasyOC.Users")]
    [RequireFeatures("OrchardCore.Users", "EasyOC.ReplaceAction")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            var targetService = services.FirstOrDefault(x => x.ImplementationType == typeof(ScriptExternalLoginEventHandler));
            if (targetService != null)
            {
                services.Remove(targetService);
            }
            services.AddScoped<IExternalLoginEventHandler, EocScriptExternalLoginEventHandler>();


            services.ReplaceActionByActionNames<EocAccountController>(
                typeof(AccountController).FullName,
                nameof(EocAccountController.LinkLoginCallback),
                nameof(EocAccountController.ExternalLogins),
                nameof(EocAccountController.LinkExternalLogin),
                nameof(EocAccountController.ExternalLoginCallback),
                nameof(EocAccountController.RegisterExternalLogin)
                );
        }

    }
    [Feature("EasyOC.OpenId.ImplicitLogout")]
    [RequireFeatures("OrchardCore.OpenId", "EasyOC.ReplaceAction")]
    public class OpenIdStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.ReplaceActionByActionNames<EocOpenIdAccessController>(
             typeof(AccessController).FullName,
             nameof(EocOpenIdAccessController.Logout)
         );
        }
    }
}