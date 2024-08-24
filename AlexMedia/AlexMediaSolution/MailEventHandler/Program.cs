using Azure.Communication.Email;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AlexMedia.Interfaces;
using AlexMedia.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton<EmailClient>(sp =>
                  {
                      var configuration = sp.GetRequiredService<IConfiguration>();
                      string connectionString = configuration["EmailServiceConnectionString"] ?? throw new ArgumentNullException("EmailServiceConnectionString");
                      return new EmailClient(connectionString);
                  });

        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            string cosmosConnectionString = configuration["CosmosDB:ConnectionString"] ?? throw new ArgumentNullException("CosmosDB:ConnectionString");
            return new CosmosClient(cosmosConnectionString);
        });

        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var cosmosClient = sp.GetRequiredService<CosmosClient>();
            string databaseName = configuration["CosmosDB:DatabaseName"] ?? throw new ArgumentNullException("CosmosDB:DatabaseName");
            string containerName = configuration["CosmosDB:ContainerName"] ?? throw new ArgumentNullException("CosmosDB:ContainerName");
            return cosmosClient.GetContainer(databaseName, containerName);
        });

        services.AddScoped<IClientDataService, ClientDataService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IRenderService, RenderService>();
        services.AddScoped<IEmailService, EmailService>();

    })
    .Build();

host.Run();