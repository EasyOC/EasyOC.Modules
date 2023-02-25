using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "EasyOC.Deployment",
    Author = EasyOC.Shared.Constants.ManifestConstants.Author,
    Website = EasyOC.Shared.Constants.ManifestConstants.Website,
    Version = EasyOC.Shared.Constants.ManifestConstants.CurrentVersion,
    Dependencies = new[] { "OrchardCore.Deployment", "OrchardCore.Roles", "OrchardCore.Queries" },
    Description = "EasyOC.Deployment",
    Category = "Deployment"
)]
