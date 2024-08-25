using Frontend.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Azure.Storage.Blobs;
using AlexMedia.Interfaces;
using AlexMedia.Services;
using Microsoft.Azure.Cosmos;
using Azure.Messaging.ServiceBus;

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

// Inject BlobContainerClient
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    string connectionString = configuration["AzureBlobStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureBlobStorage:ConnectionString");
    string containerName = configuration["AzureBlobStorage:ContainerName"] ?? throw new ArgumentNullException("AzureBlobStorage:ContainerName");

    var blobServiceClient = new BlobServiceClient(connectionString);
    return blobServiceClient.GetBlobContainerClient(containerName);
});

// Add Cosmos DB Client
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    string cosmosConnectionString = configuration["CosmosDB:ConnectionString"] ?? throw new ArgumentNullException("CosmosDB:ConnectionString");
    return new CosmosClient(cosmosConnectionString);
});

// Add Cosmos DB Container
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var cosmosClient = sp.GetRequiredService<CosmosClient>();
    string databaseName = configuration["CosmosDB:DatabaseName"] ?? throw new ArgumentNullException("CosmosDB:DatabaseName");
    string containerName = configuration["CosmosDB:ContainerName"] ?? throw new ArgumentNullException("CosmosDB:ContainerName");
    return cosmosClient.GetContainer(databaseName, containerName);
});

builder.Services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            string serviceBusConnectionString = configuration["ServiceBus:ConnectionString"] ?? throw new ArgumentNullException("ServiceBus:ConnectionString");
            return new ServiceBusClient(serviceBusConnectionString);
        });

// Register ServiceBusSender as a singleton
builder.Services.AddSingleton(sp =>
{
    var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
    var queueName = builder.Configuration["ServiceBus:QueueName"];
    return serviceBusClient.CreateSender(queueName); // Register ServiceBusSender
});

builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IClientDataService, ClientDataService>();

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