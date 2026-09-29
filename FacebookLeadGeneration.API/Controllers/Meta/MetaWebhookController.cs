using FacebookLeadGeneration.Application.DTOs;
using FacebookLeadGeneration.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacebookLeadGeneration.API.Controllers
{
    [ApiController]
    [Route("api/meta/webhook")]
    public class MetaWebhookController : ControllerBase
    {
        private readonly IMetaLeadService _metaLeadService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MetaWebhookController> _logger;

        public MetaWebhookController(
            IMetaLeadService metaLeadService,
            IConfiguration configuration,
            ILogger<MetaWebhookController> logger)
        {
            _metaLeadService = metaLeadService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string? mode,
            [FromQuery(Name = "hub.verify_token")] string? verifyToken,
            [FromQuery(Name = "hub.challenge")] string? challenge)
        {
            var configuredToken =
                _configuration["Meta:WebhookVerifyToken"];

            if (mode == "subscribe" &&
                verifyToken == configuredToken)
            {
                _logger.LogInformation(
                    "Meta webhook verification successful.");

                return Ok(challenge);
            }

            _logger.LogWarning(
                "Meta webhook verification failed.");

            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook(
            [FromBody] MetaWebhookRequestDto request)
        {
            try
            {
                _logger.LogInformation(
                    "Meta webhook notification received.");

                await _metaLeadService.ProcessWebhookAsync(request);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing Meta webhook.");

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Error while processing Meta webhook."
                    });
            }
        }

        [HttpGet("lead/{leadgenId}")]
        public async Task<IActionResult> GetLead(
            string leadgenId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(leadgenId))
                {
                    return BadRequest(new
                    {
                        message = "Lead ID is required."
                    });
                }

                var lead =
                    await _metaLeadService.GetLeadDetailsAsync(
                        leadgenId);

                return Ok(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while retrieving Meta lead. Lead ID: {LeadId}",
                    leadgenId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Error while retrieving Meta lead.",
                        error = ex.Message,
                        innerError = ex.InnerException?.Message
                    });
            }
        }

        [HttpPost("process-lead/{leadgenId}")]
        public async Task<IActionResult> ProcessLead(
            string leadgenId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(leadgenId))
                {
                    return BadRequest(new
                    {
                        message = "Lead ID is required."
                    });
                }

                var databaseLeadId =
                    await _metaLeadService.ProcessLeadAsync(
                        leadgenId);

                return Ok(new
                {
                    message = "Meta lead processed successfully.",
                    databaseLeadId = databaseLeadId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing Meta lead. Lead ID: {LeadId}",
                    leadgenId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Error while processing Meta lead."
                    });
            }
        }
    }
}