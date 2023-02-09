using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace EasyOC.SwaggerUI
{

    [Feature(Constants.Features.Swagger)]
    public class AdminMenuSwaggerUI : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenuSwaggerUI(IStringLocalizer<AdminMenuSwaggerUI> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                var prefixUrl = GetSitePerfix();
                builder.Add(S["Configuration"], configuration => configuration
                  .Add(S["SwaggerUI"], S["SwaggerUI"].PrefixPosition(), settings => settings
                            .AddClass("SwaggerUI").Id("SwaggerUI")
                            .Url($"{prefixUrl}Swagger")
                            .Position("3")
                            .LocalNav()
                        )
                  );
            }
            return Task.CompletedTask;

        }

        private string GetSitePerfix()
        {
            var baseUrl = "/" + ShellScope.Context.Settings.RequestUrlPrefix;
            baseUrl = baseUrl.EnsureEndsWith('/');
            return baseUrl;
        }

    }
}

