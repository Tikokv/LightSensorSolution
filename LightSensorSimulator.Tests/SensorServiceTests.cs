using LightSensorSimulator.Data;
using LightSensorSimulator.SimulatorServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LightSensorSimulator.Tests
{
    public class SensorServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<SensorService>> _mockLogger;
        private readonly IOptions<SensorSetting> _sensorSettings;
        private readonly SensorService _sensorService;

        public SensorServiceTests()
        {
            // Mock IHttpClientFactory
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            // Mock ILogger
            _mockLogger = new Mock<ILogger<SensorService>>();

            // Set up sensor settings
            _sensorSettings = Options.Create(new SensorSetting
            {
                DeviceId = "testDevice",
                ServerUrl = "https://testserver.com",
                SendIntervalHours = 1,
                MeasurementIntervalMinutes = 15
            });

            // Create an instance of SensorService
            _sensorService = new SensorService(_mockHttpClientFactory.Object, _mockLogger.Object, _sensorSettings);
        }

        [Fact]
        public void StartSimulation_StartsTimer()
        {
            // Act
            _sensorService.StartSimulation();

            // Assert
            _mockLogger.Verify(
                log => log.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, t) => obj.ToString().Contains("Simulation started.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void StopSimulation_StopsTimer()
        {
            // Arrange
            _sensorService.StartSimulation();

            // Act
            _sensorService.StopSimulation();

            // Assert
            _mockLogger.Verify(
                log => log.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, t) => obj.ToString().Contains("Simulation stopped.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendTelemetryData_SendsHttpRequest()
        {
            // Arrange
            var httpClientHandler = new MockHttpMessageHandler();
            httpClientHandler.When($"{_sensorSettings.Value.ServerUrl}/devices/{_sensorSettings.Value.DeviceId}/telemetry")
                             .Respond(HttpStatusCode.OK);

            var httpClient = new HttpClient(httpClientHandler);
            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            _sensorService.StartSimulation();

            // We can't directly invoke SendTelemetryData since it's private and called by a timer.
            // However, we can verify that an HTTP request was made and the logger captured it correctly.
            await Task.Delay(1000);  // Small delay to allow the timer to trigger

            // Assert
            _mockLogger.Verify(
                log => log.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((obj, t) => obj.ToString().Contains("Telemetry data sent successfully.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
