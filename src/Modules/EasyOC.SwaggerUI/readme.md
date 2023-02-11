## [EasyOC.SwaggerUI](src/Modules/EasyOC.SwaggerUI)
[![NuGet](https://img.shields.io/nuget/v/EasyOC.SwaggerUI.svg)](https://www.nuget.org/packages/EasyOC.SwaggerUI)

- Adding Swagger UI support to OrchardCore
- Integration OpenId code flow authentication


## How to use?

1. Enable "EasyOC.SwaggerUI" in Feature List
1. Run “EasyOC SwaggerUI Auth” in Recipes List
1. Config Authentication
- In Admin menus , Open : Security > OpenID Connect > Applications 
- Locate SwaggerClient in the list of applications, Edit
- Update your Redirect Uris 
- 
 You need keep the postfix , and use your domain and site Prefix to instead "http://localhost:2919/" 
 - In Admin menus , Open : Configuration > SwaggerUI  , Click `Authrize` on the right
 
 input SwaggerClient in client_id field,keep the client_secret empty,  Click Authrize
![image](https://user-images.githubusercontent.com/15613121/218245730-a2ce5bfa-6400-464c-8975-4e5d1365303f.png)

 

