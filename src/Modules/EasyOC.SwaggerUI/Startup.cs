using EasyOC.SwaggerUI.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace EasyOC.SwaggerUI
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenuSwaggerUI>();

            // 注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSwaggerGen(options =>
            {
                options.UseInlineDefinitionsForEnums();
                options.DocumentFilter<SwaggerDocumentFilter>();
                options.CustomSchemaIds(type => type.Name);
                options.ParameterFilter<SwaggerEnumParameterFilter>();
                options.SchemaFilter<SwaggerEnumSchemaFilter>();
                options.OperationFilter<SwaggerOperationIdFilter>();
                options.OperationFilter<SwaggerOperationFilter>();
                options.CustomDefaultSchemaIdSelector();
                var baseUrl = GetSitePerfix();
                // Swagger 2.0 UI requires a trailing slash
                var AuthorizationUrl = new Uri($"{baseUrl}connect/authorize", UriKind.RelativeOrAbsolute);
                var TokenUrl = new Uri($"{baseUrl}connect/token", UriKind.RelativeOrAbsolute);
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {

                    Type = SecuritySchemeType.OAuth2,
                    Description = "OpenID Connect",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Flows = new OpenApiOAuthFlows()
                    {
                         AuthorizationCode = new OpenApiOAuthFlow()
                        {
                           
                            Scopes = new Dictionary<string, string>
                            {
                                {
                                    "openid", "OpenID"
                                },
                                {
                                    "profile", "Profile"
                                }
                            },
                            AuthorizationUrl = AuthorizationUrl,
                            TokenUrl = TokenUrl,
                        },
                    }
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme, Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "EasyOC Dynamic WebApi",
                    Version = "v1"
                });
                // TODO:一定要返回true！
                options.DocInclusionPredicate((docName, description) => true);

                //xml 配置文档
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var xmlDocFiles = Directory.GetFiles(baseDirectory, "*.xml");
                foreach (var xmlFile in xmlDocFiles)
                {
                    var asName = Path.GetFileNameWithoutExtension(xmlFile);
                    if (File.Exists(Path.Combine(baseDirectory, asName + ".dll")))
                    {
                        var xmlPath = Path.Combine(baseDirectory, xmlFile);

                        options.IncludeXmlComments(xmlPath);
                    }
                }
            }).AddSwaggerGenNewtonsoftSupport();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseSwagger();
            //启用中间件服务对swagger-ui，指定Swagger JSON终结点
            app.UseSwaggerUI(options =>
            {
                var env = serviceProvider.GetRequiredService<IHostEnvironment>();
                if (env.IsDevelopment())
                {
                    options.OAuthClientId("SwaggerClient");
                    // options.OAuthClientSecret(_shellConfiguration["AuthServer:SwaggerClientSecret"]);
                    //options.OAuth2RedirectUrl(_shellConfiguration["AuthServer:SwaggerOAuth2RedirectUrl"]);
                }
                var baseUrl = GetSitePerfix();

                options.SwaggerEndpoint($"{baseUrl}swagger/v1/swagger.json", "EasyOC WebApi");
                options.OAuthScopes("openid", "profile");

            });
        }

        private string GetSitePerfix()
        {
            var baseUrl = "/" + ShellScope.Context.Settings.RequestUrlPrefix;
            baseUrl = baseUrl.EnsureEndsWith('/');
            return baseUrl;
        }
    }
}
