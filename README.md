# Orchard Core EasyOC.Modules

The repository contains a set of modules for the [Orchard Core CMS](https://github.com/OrchardCMS/OrchardCore),  and the repository is working to make Orchard Core easier to use.

QQ 群:877196442 或者点击这里
[![Orchard Core CN 中文讨论组](https://docs.orchardcore.net/en/latest/docs/assets/images/orchard-core-cn-community-logo.png)](https://shang.qq.com/wpa/qunwpa?idkey=48721591a71ee7586316604a7a4ee99d26fd977c6120370a06585085a5936f62) 


## Orchard Core Reference

This repositry is referencing a stable build of Orchard Core ([`1.8.0`](https://www.nuget.org/packages/OrchardCore.Module.Targets/1.6.0)).



## [EasyOC.AssemblyLoader](src/Modules/EasyOC.AssemblyLoader)
[![NuGet Badge](https://buildstats.info/nuget/EasyOC.AssemblyLoader?includePreReleases=true)](https://www.nuget.org/packages/EasyOC.AssemblyLoader)

Load assemblies from config file

appsettings.json
```json
"OrchardCore":{
    "ExternalLib": {
      "Paths": [ 
          "ExternalLibs", // 动态库路径 相对于App_Data 路径,或指定完整路径
          "D:\\project\\bin\\net8.0"// Your external lib 
      ]
    },
}

```


## [EasyOC.ActionReplace](src/Modules/EasyOC.ReplaceAction)
[![NuGet Badge](https://buildstats.info/nuget/EasyOC.ReplaceAction?includePreReleases=true)](https://www.nuget.org/packages/EasyOC.ReplaceAction)

Replace the implementation of the specified controller action in OrchardCore

## [EasyOC.Users](src/Modules/EasyOC.Users)
[![NuGet Badge](https://buildstats.info/nuget/EasyOC.Users?includePreReleases=true)](https://www.nuget.org/packages/EasyOC.Users)

This module implements some controller action replacement with the help of `EasyOC.ReplaceAction`.

## [EasyOC.SwaggerUI](src/Modules/EasyOC.SwaggerUI)
[![NuGet Badge](https://buildstats.info/nuget/EasyOC.SwaggerUI?includePreReleases=true)](https://www.nuget.org/packages/EasyOC.SwaggerUI)


- Adding Swagger UI support to OrchardCore
- Integration OpenId code flow authentication

## [EasyOC.Workflows](src/Modules/EasyOC.Workflows)
[![NuGet Badge](https://buildstats.info/nuget/EasyOC.Workflows?includePreReleases=true)](https://www.nuget.org/packages/EasyOC.Workflows)

### EasyOC.Workflows.Timers
Make the Timer Workflow Activty support site time zone.


## [EasyOC.Deployment](src/Modules/EasyOC.Deployment)
[![NuGet Badge](https://buildstats.info/nuget/EasyOC.Deployment?includePreReleases=true)](https://www.nuget.org/packages/EasyOC.Deployment)

Improve the OC `Deployment` feature

 


## 鸣谢

感谢 JetBrains 为开源项目提供[免费赞助](https://www.jetbrains.com/community/opensource/#support)的的 [Rider](https://www.jetbrains.com/zh-cn/rider/) 等 IDE 的授权

![Rider](https://resources.jetbrains.com/storage/products/company/brand/logos/Rider.svg)



