using FacebookLeadGeneration.Application.DTOs;

namespace FacebookLeadGeneration.Application.Interfaces.Services
{
    public interface IMetaLeadService
    {
        Task ProcessWebhookAsync(MetaWebhookRequestDto request);

        Task<object?> GetLeadDetailsAsync(string leadgenId);

        Task<int> ProcessLeadAsync(string leadgenId);

       // Task<object?> GetFormLeadsAsync(string formId);
    }
}