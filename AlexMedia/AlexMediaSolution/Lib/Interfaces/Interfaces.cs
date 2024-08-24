
using System.Collections.Generic;
using System.Threading.Tasks;
using WINX.Models;

namespace WINX.Interfaces
{

    // Interfaces
    public enum EmailStatus
    {
        Succeeded,
        Failed,
        Pending
    }


    public interface IEmailService
    {
        Task<EmailStatus> SendEmailAsync(EmailNotification notification, string recipientAddress, string renderedEmail);
    }




    public interface IClientDataService
    {
        Task UpsertClientDataAsync(ClientData clientData);

        Task<ClientData> GetClientDataAsync(string clientId);
    }

    public interface ITemplateService
    {
        Task DeleteTemplateAsync(string clientId, string templateId);
        Task<List<EmailTemplate>> GetClientTemplatesAsync(string clientId);
        Task<string> GetTemplateAsync(string clientId, string templateId1);
        Task<EmailTemplate> SaveTemplateAsync(EmailTemplate template);
    }

    public interface IRenderService
    {
        Task<string> RenderTemplateAsync(string template, string marketingData);
    }
}
