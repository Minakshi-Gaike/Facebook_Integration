namespace FacebookLeadGeneration.Application.Interfaces.Repositories
{
    public interface IMetaLeadRepository
    {
        Task<int> InsertLeadAsync(
            string? candidateName,
            string? emailAddress,
            string? mobileNumber,
            string? trainingType,
            string? description,
            string? status,
            DateTime? leadDate,
            int? sourceId);
    }
}