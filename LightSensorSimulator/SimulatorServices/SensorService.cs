using System.Text.Json;
using System.Text;
using LightSensorSimulator.Data;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace LightSensorSimulator.SimulatorServices
{
    public class SensorService : BackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SensorService> _logger;
        private readonly SensorSetting _sensorSettings;
        private Timer _timer;
        private bool _isRunning;

        public SensorService(IHttpClientFactory httpClientFactory, ILogger<SensorService> logger, IOptions<SensorSetting> sensorSettings)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _sensorSettings = sensorSettings.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public void StartSimulation()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _timer = new Timer(SendTelemetryData, null, TimeSpan.Zero, TimeSpan.FromHours(_sensorSettings.SendIntervalHours));
                _logger.LogInformation("Simulation started.");
            }
        }

        public void StopSimulation()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _timer?.Change(Timeout.Infinite, 0);
                _logger.LogInformation("Simulation stopped.");
            }
        }

        private async void SendTelemetryData(object state)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var deviceId = _sensorSettings.DeviceId; 
            var serverUrl = _sensorSettings.ServerUrl; 

            var telemetries = new List<LuxData>();
            for (int i = 0; i < 4; i++) 
            {
                var telemetry = new LuxData
                {
                    Illum = Math.Round((new Random().NextDouble() * 100 + 100) * 2, MidpointRounding.AwayFromZero) / 2,
                    Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - i * _sensorSettings.MeasurementIntervalMinutes * 60 // Use configured measurement interval
                };
                telemetries.Add(telemetry);
            }

            var jsonContent = JsonSerializer.Serialize(telemetries);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.PostAsync($"{serverUrl}/devices/{deviceId}/telemetry", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Telemetry data sent successfully.");
                }
                else
                {
                    _logger.LogWarning($"Failed to send telemetry data. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending telemetry data: {ex.Message}");
            }
        }
    }
}
