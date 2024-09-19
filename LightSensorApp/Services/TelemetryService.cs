using LightSensorApp.Db;
using LightSensorApp.DTOs;
using LightSensorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LightSensorApp.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly LightSensorContext _context;
        private readonly ILogger<TelemetryService> _logger;

        public TelemetryService(LightSensorContext context, ILogger<TelemetryService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<TelemetryResponseDto>> StoreTelemetryData(string deviceId, IEnumerable<TelemetryRequestDto> telemetryData)
        {
            if (telemetryData == null || !telemetryData.Any())
            {
                _logger.LogWarning($"No telemetry data provided for device: {deviceId}");
            }

            var telemetryList = telemetryData.Select(data => new DeviceTelemetry
            {
                DeviceId = deviceId,
                Illum = data.Illum,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(data.Time).UtcDateTime
            }).ToList();

            foreach (var telemetry in telemetryList)
            {
                _logger.LogInformation($"Storing telemetry for DeviceId: {telemetry.DeviceId}, Illum: {telemetry.Illum}, Timestamp: {telemetry.Timestamp}");
            }

            _context.DeviseTelemetries.AddRange(telemetryList);
            var telemetryData1 = await _context.DeviseTelemetries
                                      .Where(t => t.DeviceId == deviceId)
                                      .ToListAsync();

            await _context.SaveChangesAsync();

            var responseData = telemetryData1.Select(t => new TelemetryResponseDto
            {
                Illum = t.Illum,
                Time = new DateTimeOffset(t.Timestamp).ToUnixTimeSeconds()
            }).ToList();

            return responseData;
        }

        public async Task<List<StatisticsResponseDto>> GetDeviceStatistics(string deviceId)
        {
            var telemetryData = await _context.DeviseTelemetries
            .Where(d => d.DeviceId == deviceId && d.Timestamp > DateTime.UtcNow.AddDays(-30))
            .ToListAsync();

            var statistics = telemetryData
                .GroupBy(d => d.Timestamp.Date)
                .Select(g => new StatisticsResponseDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    MaxIlluminance = g.Max(d => d.Illum)
                })
                .OrderByDescending(g => g.Date)
                .ToList();

            return statistics;
        }
    }
}
