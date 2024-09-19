using LightSensorApp.Controllers;
using LightSensorApp.DTOs;
using LightSensorApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightSensorApp.Test
{
    public class DeviceControllerTests
    {
        private readonly Mock<ITelemetryService> _mockTelemetryService;
        private readonly Mock<ILogger<DeviceController>> _mockLogger;
        private readonly DeviceController _controller;

        public DeviceControllerTests()
        {
            _mockTelemetryService = new Mock<ITelemetryService>();
            _mockLogger = new Mock<ILogger<DeviceController>>();
            _controller = new DeviceController(_mockTelemetryService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task PostTelemetry_ReturnsBadRequest_WhenTelemetriesAreNull()
        {
            var result = await _controller.PostTelemetry("device1", null);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid telemetry data.", badRequestResult.Value);
        }

        [Fact]
        public async Task PostTelemetry_ReturnsBadRequest_WhenTelemetriesAreEmpty()
        {
            var result = await _controller.PostTelemetry("device1", new List<TelemetryRequestDto>());
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid telemetry data.", badRequestResult.Value);
        }

        [Fact]
        public async Task PostTelemetry_ReturnsOk_WhenTelemetriesAreValid()
        {
            var telemetries = new List<TelemetryRequestDto>
            {
                new TelemetryRequestDto { Illum = 100 }
            };

            _mockTelemetryService.Setup(s => s.StoreTelemetryData(It.IsAny<string>(), It.IsAny<List<TelemetryRequestDto>>()))
                                 .ReturnsAsync(new List<TelemetryResponseDto>());

            var result = await _controller.PostTelemetry("device1", telemetries);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<TelemetryResponseDto>>(okResult.Value);
        }

        [Fact]
        public async Task PostTelemetry_ReturnsBadRequest_WhenArgumentExceptionIsThrown()
        {
            _mockTelemetryService.Setup(s => s.StoreTelemetryData(It.IsAny<string>(), It.IsAny<List<TelemetryRequestDto>>()))
                                 .ThrowsAsync(new ArgumentException("Invalid device ID"));

            var result = await _controller.PostTelemetry("device1", new List<TelemetryRequestDto> { new TelemetryRequestDto { Illum = 100 } });

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid device ID", badRequestResult.Value);
        }

        [Fact]
        public async Task GetStatistics_ReturnsOk_WithStatistics()
        {
            var expectedStatistics = new List<StatisticsResponseDto>
            {
                new StatisticsResponseDto { Date = "2024-09-18", MaxIlluminance = 200 }
            };

            _mockTelemetryService.Setup(s => s.GetDeviceStatistics(It.IsAny<string>()))
                                 .ReturnsAsync(expectedStatistics);

            var result = await _controller.GetStatistics("device1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var statistics = Assert.IsType<List<StatisticsResponseDto>>(okResult.Value);
            Assert.Single(statistics);
            Assert.Equal(expectedStatistics[0].MaxIlluminance, statistics[0].MaxIlluminance);
        }
    }
}