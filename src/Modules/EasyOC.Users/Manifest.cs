using OrchardCore.Modules.Manifest;
[assembly: Module(
    Name = "EasyOC.Users",
    Author = "Tony Han",
    Website = "https://github.com/EasyOC/EasyOC.Modules"
)]

[assembly: Feature(
    Id = "EasyOC.Users",
    Name = "EasyOC.Users",
    Dependencies = new[] { "OrchardCore.Users", "EasyOC.ReplaceAction" },
    Description = "Make the UserLogin Script supports Update custom user properties and Update `UserClaims`",
    Category = "Content Management"
)]


[assembly: Feature(
    Id = "EasyOC.OpenId",
    Name = "Implicit logout",
    Dependencies = new[] { "OrchardCore.OpenId", "EasyOC.ReplaceAction" },
    Description = "Confirmation is no longer required when logging out using OpenId",
    Category = "Content Management"
)]
