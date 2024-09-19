using LightSensorSimulator.Controllers;
using LightSensorSimulator.SimulatorServices;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using LightSensorSimulator.Data;
using Microsoft.Extensions.Options;

namespace LightSensorSimulator.Tests
{
    public class SensorControllerTests
    {
        private readonly Mock<SensorService> _mockSensorService;
        private readonly SensorController _controller;

        public SensorControllerTests()
        {
            // Mock the dependencies for SensorService
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var loggerMock = new Mock<ILogger<SensorService>>();
            var sensorSettingsMock = new Mock<IOptions<SensorSetting>>();

            // Set up mock for IOptions<SensorSetting> to return default settings
            sensorSettingsMock.Setup(s => s.Value).Returns(new SensorSetting());

            // Create a mock for SensorService with the required constructor parameters
            _mockSensorService = new Mock<SensorService>(
                httpClientFactoryMock.Object,
                loggerMock.Object,
                sensorSettingsMock.Object);

            // Create an instance of SensorController with the mocked service
            _controller = new SensorController(_mockSensorService.Object);
        }

        [Fact]
        public void StartSimulation_ReturnsOkResult()
        {
            var result = _controller.StartSimulation();

            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal("Simulation started.", okResult.Value);
        }

        [Fact]
        public void StopSimulation_ReturnsOkResult()
        {
            var result = _controller.StopSimulation();

            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal("Simulation stopped.", okResult.Value);
        }
    }
}