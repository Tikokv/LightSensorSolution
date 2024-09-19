using LightSensorApp.DTOs;
using LightSensorApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace LightSensorApp.Controllers
{
    [ApiController]
    [Route("devices/{deviceId}")]
    public class DeviceController : ControllerBase
    {
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(ITelemetryService telemetryService, ILogger<DeviceController> logger)
        {
            _telemetryService = telemetryService;
            _logger = logger;
        }

        [HttpPost("telemetry")]
        public async Task<IActionResult> PostTelemetry(string deviceId, [FromBody] List<TelemetryRequestDto> telemetries)
        {
            if (telemetries == null || !telemetries.Any())
            {
                _logger.LogWarning("Received invalid telemetry data.");
                return BadRequest("Invalid telemetry data.");
            }

            foreach (var telemetry in telemetries)
            {
                _logger.LogInformation($"Received telemetry - Illuminance: {telemetry.Illum}, Time: {telemetry.Time}");
            }

            try
            {
                var telemetryData = await _telemetryService.StoreTelemetryData(deviceId, telemetries);
                return Ok(telemetryData);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Bad request: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(string deviceId)
        {
            var statistics = await _telemetryService.GetDeviceStatistics(deviceId);
            _logger.LogInformation("Maximal illuminance was retrieved");
            return Ok(statistics);
        }
    }
}