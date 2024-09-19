using LightSensorApp.DTOs;
using LightSensorApp.Models;

namespace LightSensorApp.Services
{
    public interface ITelemetryService
    {
        Task<List<TelemetryResponseDto>> StoreTelemetryData(string deviceId, IEnumerable<TelemetryRequestDto> telemetryData);

        Task<List<StatisticsResponseDto>> GetDeviceStatistics(string deviceId);
    }
}
