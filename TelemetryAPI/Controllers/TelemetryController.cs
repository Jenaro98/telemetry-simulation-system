using Microsoft.AspNetCore.Mvc;
using TelemetryAPI.Models;
using TelemetryAPI.Repositories;
using TelemetryAPI.Services;
using System.Text.Json;
using MongoDB.Bson;

namespace TelemetryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryRepository _repository;
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(
        ITelemetryRepository repository, 
        IRabbitMQService rabbitMQService,
        ILogger<TelemetryController> logger)
    {
        _repository = repository;
        _rabbitMQService = rabbitMQService;
        _logger = logger;
    }

    [HttpPost("ingest")]
    public async Task<IActionResult> IngestTelemetry([FromBody] TelemetryData telemetryData)
    {
        try
        {
            // Set timestamp if not provided
            if (telemetryData.Timestamp == default)
            {
                telemetryData.Timestamp = DateTime.UtcNow;
            }

            // Convert AdditionalData to ensure it's serializable
            if (telemetryData.AdditionalData != null)
            {
                var convertedData = new Dictionary<string, object>();
                foreach (var kvp in telemetryData.AdditionalData)
                {
                    // Convert JsonElement to proper types
                    if (kvp.Value is JsonElement jsonElement)
                    {
                        convertedData[kvp.Key] = jsonElement.ValueKind switch
                        {
                            JsonValueKind.String => jsonElement.GetString() ?? "",
                            JsonValueKind.Number => jsonElement.TryGetInt32(out var intVal) ? intVal : jsonElement.GetDouble(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            _ => jsonElement.ToString()
                        };
                    }
                    else
                    {
                        convertedData[kvp.Key] = kvp.Value;
                    }
                }
                telemetryData.AdditionalData = convertedData;
            }

            // Store in database
            var id = await _repository.CreateAsync(telemetryData);
            
            // Publish to RabbitMQ for async processing
            var message = JsonSerializer.Serialize(telemetryData);
            _rabbitMQService.PublishMessage(message);

            _logger.LogInformation("Telemetry data ingested for device {DeviceId}", telemetryData.DeviceId);

            return Ok(new { Id = id, Message = "Telemetry data ingested successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting telemetry data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestTelemetry([FromQuery] string? deviceId = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(deviceId))
            {
                var latest = await _repository.GetLatestAsync(deviceId);
                if (latest == null)
                {
                    return NotFound($"No telemetry data found for device {deviceId}");
                }
                return Ok(latest);
            }
            else
            {
                var allLatest = await _repository.GetAllLatestAsync();
                return Ok(allLatest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving latest telemetry data");
            return StatusCode(500, "Internal server error");
        }
    }
}
