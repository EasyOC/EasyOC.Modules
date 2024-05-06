using EasyOC.AssemblyLoader;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using System.Reflection;

namespace EasyOC
{
    public static class AssemblyLoaderExtensions
    {
        public static WebApplicationBuilder LoadExternalLibs(this WebApplicationBuilder builder)
        {

            LoadAssembliesAndTypes(builder);
            builder.Services.AddSingleton<IModuleNamesProvider, ExternalModuleNamesProvider>();
            return builder;
        }

        public static void LoadAssembliesAndTypes(WebApplicationBuilder builder)
        {
            var paths = builder.Configuration.GetSection("OrchardCore:ExternalLib:Paths").Get<string[]>();
            if (paths is null || !paths.Any())
            {
                return;
            }

            var libsRoot = RootPathFactory(builder);

            foreach (var libraryPath in paths)
            {
                var folder = libraryPath;
                if (!Path.IsPathRooted(libraryPath))
                {
                    folder = Path.Combine(libsRoot, libraryPath);
                }
                // 加载库文件
                if (Directory.Exists(folder))
                {
                    foreach (var assemblyFilePath in Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories))
                    {
                        try
                        {
                            var assembly = Assembly.LoadFrom(assemblyFilePath);
                            if (!ExternalModuleNamesProvider.ExternalAssemblies.Contains(assembly))
                            {
                                ExternalModuleNamesProvider.ExternalAssemblies.Add(assembly);
                            }
                            Console.WriteLine(assembly.FullName + "已加载");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(assemblyFilePath + "程序集加载失败");
                            Console.WriteLine($"{ex}");
                        }
                    }
                }
            }
        }
        static Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // 从已加载的程序集中查找依赖项
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == args.Name)
                {
                    // 找到了相应的依赖项
                    return assembly;
                }
            }

            // 未找到相应的依赖项
            return null;
        }
        private static string RootPathFactory(WebApplicationBuilder builder)
        {
            var contentRootPath = builder.Environment.ContentRootPath;
            var libsRoot = Path.Combine(contentRootPath, "App_Data");

            var appData = Environment.GetEnvironmentVariable("ORCHARD_APP_DATA");
            if (!string.IsNullOrEmpty(appData))
            {
                libsRoot = Path.Combine(contentRootPath, appData);
            }
            else
            {
                libsRoot = Path.Combine(contentRootPath, "App_Data");
            }
            return libsRoot;
        }
    }
}
