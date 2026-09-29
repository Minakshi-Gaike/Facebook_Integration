using System.Text.Json;
using FacebookLeadGeneration.Application.DTOs;
using FacebookLeadGeneration.Application.Interfaces.Repositories;
using FacebookLeadGeneration.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FacebookLeadGeneration.Application.Services
{
    public class MetaLeadService : IMetaLeadService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MetaLeadService> _logger;
        private readonly IMetaLeadRepository _metaLeadRepository;

        public MetaLeadService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<MetaLeadService> logger,
            IMetaLeadRepository metaLeadRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _metaLeadRepository = metaLeadRepository;
        }

        public async Task ProcessWebhookAsync(
            MetaWebhookRequestDto request)
        {
            if (request.Entry == null)
            {
                _logger.LogWarning(
                    "Meta webhook contains no entries.");

                return;
            }

            foreach (var entry in request.Entry)
            {
                if (entry.Changes == null)
                    continue;

                foreach (var change in entry.Changes)
                {
                    var leadgenId = change.Value?.LeadgenId;

                    if (string.IsNullOrWhiteSpace(leadgenId))
                        continue;

                    _logger.LogInformation(
                        "Lead received from Meta. Lead ID: {LeadId}",
                        leadgenId);

                    try
                    {
                        var databaseLeadId =
                            await ProcessLeadAsync(leadgenId);

                        _logger.LogInformation(
                            "Meta lead saved successfully. Database Lead ID: {DatabaseLeadId}",
                            databaseLeadId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error while processing Meta Lead ID: {LeadId}",
                            leadgenId);

                        throw;
                    }
                }
            }
        }

        public async Task<object?> GetLeadDetailsAsync(
            string leadgenId)
        {
            var accessToken =
                _configuration["Meta:PageAccessToken"];

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException(
                    "Meta Page Access Token is not configured.");
            }

            var graphApiVersion =
                _configuration["Meta:GraphApiVersion"]
                ?? "v26.0";

            var url =
                $"https://graph.facebook.com/{graphApiVersion}/{leadgenId}" +
                $"?fields=id,created_time,field_data" +
                $"&access_token={Uri.EscapeDataString(accessToken)}";

            _logger.LogInformation(
                "Requesting lead details from Meta Graph API for Lead ID: {LeadId}",
                leadgenId);

            using var response =
                await _httpClient.GetAsync(url);

            var responseContent =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Meta Graph API returned {StatusCode}: {Response}",
                    response.StatusCode,
                    responseContent);

                throw new HttpRequestException(
                    $"Meta Graph API request failed. " +
                    $"Status: {response.StatusCode}. " +
                    $"Response: {responseContent}");
            }

            return responseContent;
        }

        public async Task<int> ProcessLeadAsync(
            string leadgenId)
        {
            if (string.IsNullOrWhiteSpace(leadgenId))
            {
                throw new ArgumentException(
                    "Lead ID is required.",
                    nameof(leadgenId));
            }

            _logger.LogInformation(
                "Processing Meta Lead ID: {LeadId}",
                leadgenId);

            var leadJson =
                await GetLeadDetailsAsync(leadgenId);

            if (leadJson == null)
            {
                throw new InvalidOperationException(
                    "Unable to retrieve lead details from Meta.");
            }

            var databaseLeadId =
                await SaveLeadToDatabaseAsync(leadJson);

            _logger.LogInformation(
                "Meta Lead ID {LeadId} saved with Database Lead ID {DatabaseLeadId}",
                leadgenId,
                databaseLeadId);

            return databaseLeadId;
        }

        private async Task<int> SaveLeadToDatabaseAsync(
            object leadJson)
        {
            using var document =
                JsonDocument.Parse(leadJson.ToString()!);

            var root =
                document.RootElement;

            string? candidateName = null;
            string? emailAddress = null;
            string? mobileNumber = null;
            string? trainingType = null;

            DateTime? leadDate = null;

            if (root.TryGetProperty(
                    "created_time",
                    out var createdTimeProperty))
            {
                if (DateTime.TryParse(
                        createdTimeProperty.GetString(),
                        out var parsedDate))
                {
                    leadDate = parsedDate;
                }
            }

            if (root.TryGetProperty(
                    "field_data",
                    out var fieldDataProperty))
            {
                foreach (var field in fieldDataProperty.EnumerateArray())
                {
                    if (!field.TryGetProperty(
                            "name",
                            out var nameProperty))
                    {
                        continue;
                    }

                    var fieldName =
                        nameProperty.GetString();

                    if (!field.TryGetProperty(
                            "values",
                            out var valuesProperty))
                    {
                        continue;
                    }

                    var value =
                        valuesProperty.GetArrayLength() > 0
                            ? valuesProperty[0].GetString()
                            : null;

                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    switch (fieldName?.ToLowerInvariant())
                    {
                        case "full_name":
                        case "name":
                        case "candidate_name":
                            candidateName = value;
                            break;

                        case "email":
                        case "email_address":
                            emailAddress = value;
                            break;

                        case "phone_number":
                        case "phone":
                        case "mobile_number":
                            mobileNumber = value;
                            break;

                        case "interested_course":
                        case "course":
                        case "course_name":
                        case "training_type":
                            trainingType = value;
                            break;
                    }
                }
            }

            var databaseLeadId =
                await _metaLeadRepository.InsertLeadAsync(
                    candidateName,
                    emailAddress,
                    mobileNumber,
                    trainingType,
                    "Lead captured from Meta Lead Form",
                    "Pending",
                    leadDate ?? DateTime.Now,
                    2);

            return databaseLeadId;
        }
    }
}