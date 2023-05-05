using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);


builder.Services
     .Configure<IdentityOptions>(options =>
     {
         options.Password.RequireDigit = false;
         options.Password.RequireLowercase = false;
         options.Password.RequireUppercase = false;
         options.Password.RequireNonAlphanumeric = false;
         options.Password.RequiredUniqueChars = 3;
         options.Password.RequiredLength = 6;
     })
    .AddOrchardCms()
// // Orchard Specific Pipeline
// .ConfigureServices( services => {
// })
// .Configure( (app, routes, services) => {
// })
;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseOrchardCore();

app.Run();
