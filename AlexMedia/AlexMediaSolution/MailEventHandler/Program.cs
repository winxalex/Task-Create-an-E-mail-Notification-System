using Azure.Communication.Email;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WINX.Interfaces;
using WINX.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddSingleton<EmailClient>(sp =>
                  {
                      string connectionString = Environment.GetEnvironmentVariable("EmailServiceConnectionString");
                      return new EmailClient(connectionString);
                  });

        services.AddScoped<IClientDataService, ClientDataService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IRenderService, RenderService>();
        services.AddScoped<IEmailService, EmailService>();

    })
    .Build();

host.Run();
