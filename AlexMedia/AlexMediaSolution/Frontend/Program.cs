using Frontend.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


// Add Azure AD Authentication
builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
//});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ClientOrAdmin", policy =>
        policy.RequireRole("Client", "Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

//using Frontend.Components;
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.AspNetCore.Components.Authorization;
//using Microsoft.Identity.Web;
//using Microsoft.Identity.Web.UI;
////using AlexMedia.EmailNotificationSystem.Services;
////using AlexMedia.EmailNotificationSystem.Data;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddRazorPages();
//builder.Services.AddServerSideBlazor();
////builder.Services.AddSingleton<ClientDataService>();
////builder.Services.AddSingleton<TemplateService>();
////builder.Services.AddSingleton<CampaignService>();

//// Add Azure AD Authentication
//builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd")
//    .EnableTokenAcquisitionToCallDownstreamApi()
//    .AddInMemoryTokenCaches();

//builder.Services.AddControllersWithViews()
//    .AddMicrosoftIdentityUI();

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
//});

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();
//app.MapBlazorHub();
////app.MapFallbackToPage("/_Host");

//app.Run();
