using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "ReplaceAction",
    Author = "Tony Han",
    Website = "https://github.com/EasyOC/EasyOC.Modules",
    Version = "1.0.1"
)]
[assembly: Feature(
    Id = "EasyOC.ReplaceAction",
    Name = "ReplaceAction",
    Description = "Override the controller action in OrchardCore with the specified controller method.",
    Category = "Development"
)]