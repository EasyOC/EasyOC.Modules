
## EasyOC.Users

This module implements some controller action replacement with the help of `EasyOC.ReplaceAction`.

The UserLogin Script in OrchardCore now supports the following features

- [x] Update custom user settings properties
- [x] Update `UserClaims`

## Nuget

[![NuGet Badge](https://buildstats.info/nuget/EasyOC.Users?includePreReleases=true)](https://www.nuget.org/packages/EasyOC.Users)


## Orchard Core Reference

This module is referencing a stable build of Orchard Core ([`1.5.0`](https://www.nuget.org/packages/OrchardCore.Module.Targets/1.5.0)).





## How to use

1. Add package reference
```
dotnet add package EasyOC.Users
```
2. Enable the `EasyOC.Users` feature

3. Update your login script to look like this

```js
log("Warning", "Login-ExternalLoginProvider "+ context.loginProvider +  JSON.stringify(context));

switch (context.loginProvider) {
    case "OpenIdConnect": 
        //update roles
        context.rolesToAdd.push("Role1"); 
        context.rolesToAdd.push("Role2");
        context.rolesToRemove.push("Role2");
        //update custom user settings properties
        // 不需要列出所有属性，这些修改的信息将会合并到 用户扩展信息中
        context.propertiesToUpdate={
            UserProfileInternal:{
                UserProfilePart:{ 
                    DisplayName:{
                        Text:context.externalClaims.find(x=>x.type=="given_name")?.value
                    }
                }
            }
        }
        //add or update userClaims 
        // Multiple logins, the same value will not trigger repeated updates
        context.claimsToAdd={given_name:"San, Zhang"};
        // Removes an existing claim, if it exists 
        context.claimsToRemove=["mobile"]
    
    break;
    default:
        log("Warning", "Provider " + context.loginProvider  + " was not handled");
        break;
}

```

This module has been override the actions below:

```C#
            services.ReplaceActionByActionNames<EocAccountController>(
                typeof(AccountController).FullName, 
                nameof(EocAccountController.LinkLoginCallback),
                nameof(EocAccountController.ExternalLogins),
                nameof(EocAccountController.LinkExternalLogin),
                nameof(EocAccountController.ExternalLoginCallback),
                nameof(EocAccountController.RegisterExternalLogin)
                );
```
