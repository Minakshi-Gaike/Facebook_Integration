using System.Text.Json.Serialization;

namespace FacebookLeadGeneration.Application.DTOs
{
    public class MetaWebhookRequestDto
    {
        public string? Object { get; set; }

        public List<MetaWebhookEntryDto>? Entry { get; set; }
    }

    public class MetaWebhookEntryDto
    {
        public string? Id { get; set; }

        public List<MetaWebhookChangeDto>? Changes { get; set; }
    }

    public class MetaWebhookChangeDto
    {
        public string? Field { get; set; }

        public MetaWebhookValueDto? Value { get; set; }
    }

    public class MetaWebhookValueDto
    {
        [JsonPropertyName("leadgen_id")]
        public string? LeadgenId { get; set; }

        [JsonPropertyName("page_id")]
        public string? PageId { get; set; }

        [JsonPropertyName("form_id")]
        public string? FormId { get; set; }

        [JsonPropertyName("ad_id")]
        public string? AdId { get; set; }

        [JsonPropertyName("adgroup_id")]
        public string? AdgroupId { get; set; }

        [JsonPropertyName("created_time")]
        public long? CreatedTime { get; set; }
    }
}