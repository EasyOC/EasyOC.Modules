## EasyOC.ReplaceAction

Replace the implementation of the specified controller action in OrchardCore

## Nuget

[![NuGet Badge](https://buildstats.info/nuget/EasyOC.ReplaceAction?includePreReleases=true)](https://www.nuget.org/packages/EasyOC.ReplaceAction)

## Orchard Core Reference

This module is referencing a stable build of Orchard Core ([`1.6.0`](https://www.nuget.org/packages/OrchardCore.Module.Targets/1.6.0)).

## How to use

1. Add package reference
```
dotnet add package EasyOC.ReplaceAction
```

The package must be added in the startup project, 

and the 'PrivateAssets="none" option must be added if referenced in another project(not a startup project)

```xml
<PackageReference Include="EasyOC.ReplaceAction" PrivateAssets="none"/>
```

2. Enable the `EasyOC.ReplaceAction` feature
![image](https://user-images.githubusercontent.com/15613121/215324237-f1b182fc-fa91-4043-9f3e-b7ccbae19a8a.png)
3. Replace the controller in OrchardCore with your own controller method

update your module's `Startup.cs`

```C#
public override void ConfigureServices(IServiceCollection services)
{
    //sample 1
    services.ReplaceAction<AccountController, EocAccountController>(nameof(EocAccountController.Login));
    //sample 2 , Avoid unnecessary package references
    services.ReplaceAction<EocAccountController>("OrchardCore.Users.Controllers.AccountController", "Login");
    //sample 3 , mapping diffrent name
    services.ReplaceAction<EocAccountController>("OrchardCore.Users.Controllers.AccountController", "ExternalLogin", "MyExternalLogin");

    //sample 4 , Advanced configuration
    services.ReplaceAction(opt =>
    {
        var type = typeof(EocAccountController);

        opt.Items.Add(new ActionReplaceOptionItem
        {
            TargetControllerFullName = typeof(AccountController).FullName,
            NewController = type,
            ActionMapping = new Dictionary<string, MethodInfo> { ["ExternalLogin"] = type.GetMethod("ExternalLogin") }
        });
    });
    //sample 5 , Advanced configuration
    services.ReplaceAction(opt =>
    {
        opt.Items.Add(new ActionReplaceOptionItem
        {
            CustomAction = (descriptor) =>
            {
                //Do whatever you want
                Console.WriteLine(descriptor.ControllerName);
            }
        });
    });
    //sample 6 
            services.ReplaceActionByActionNames<EocAccountController>(
                typeof(AccountController).FullName, 
                nameof(EocAccountController.LinkLoginCallback),
                nameof(EocAccountController.ExternalLogins),
                nameof(EocAccountController.LinkExternalLogin),
                nameof(EocAccountController.ExternalLoginCallback),
                nameof(EocAccountController.RegisterExternalLogin)
                );
}
```
