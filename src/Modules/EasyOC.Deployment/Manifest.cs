using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "EasyOC.Deployment",
    Author = EasyOC.Constants.ManifestConstants.Author,
    Website = EasyOC.Constants.ManifestConstants.Website,
    Version = EasyOC.Constants.ManifestConstants.CurrentVersion,
    Dependencies = new[] { "OrchardCore.Deployment", "OrchardCore.Roles", "OrchardCore.Queries" },
    Description = "EasyOC.Deployment",
    Category = "Deployment"
)]
