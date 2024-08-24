// Interfaces
public interface IClientDataService
{
    Task<ClientData> GetClientDataAsync(string clientId);
}

public interface ITemplateService
{
    Task<Template> GetTemplateAsync(string templateId);
}

public interface IRenderService
{
    Task<string> RenderTemplateAsync(string template, object data, object marketingData);
}
