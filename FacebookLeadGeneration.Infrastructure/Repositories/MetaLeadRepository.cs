using Dapper;
using FacebookLeadGeneration.Application.Interfaces.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FacebookLeadGeneration.Infrastructure.Repositories
{
    public class MetaLeadRepository : IMetaLeadRepository
    {
        private readonly IConfiguration _configuration;

        public MetaLeadRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> InsertLeadAsync(
            string? candidateName,
            string? emailAddress,
            string? mobileNumber,
            string? trainingType,
            string? description,
            string? status,
            DateTime? leadDate,
            int? sourceId)
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string is not configured.");
            }

            using var connection =
                new SqlConnection(connectionString);

            var parameters = new DynamicParameters();

            parameters.Add("@Action", "INSERT");
            parameters.Add("@lead_id", null);
            parameters.Add("@candidate_name", candidateName);
            parameters.Add("@email_address", emailAddress);
            parameters.Add("@mobile_number", mobileNumber);
            parameters.Add("@training_type", trainingType);
            parameters.Add("@description", description);
            parameters.Add("@status", status);
            parameters.Add("@lead_date", leadDate);
            parameters.Add("@source_id", sourceId);

            var result = await connection.QueryFirstOrDefaultAsync<int>(
                "[erpsystem].[sp_tblleads]",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }
    }
}