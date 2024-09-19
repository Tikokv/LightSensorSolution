using LightSensorApp.Db;
using LightSensorApp.DTOs;
using LightSensorApp.Models;
using LightSensorApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LightSensorApp.Test
{
    public class TelemetryServiceTests
    {
        private readonly LightSensorContext _context;
        private readonly Mock<ILogger<TelemetryService>> _mockLogger;
        private readonly TelemetryService _service;

        public TelemetryServiceTests()
        {
            var options = new DbContextOptionsBuilder<LightSensorContext>()
                .UseInMemoryDatabase($"TestDatabase{Guid.NewGuid()}")
                .Options;

            _context = new LightSensorContext(options);
            _mockLogger = new Mock<ILogger<TelemetryService>>();
            _service = new TelemetryService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task StoreTelemetryData_StoresDataSuccessfully()
        {
            var deviceId = "device1";
            var telemetryData = new List<TelemetryRequestDto>
            {
                new TelemetryRequestDto { Illum = 150, Time = new DateTimeOffset(new DateTime(2024, 9, 18)).ToUnixTimeSeconds() },
                new TelemetryRequestDto { Illum = 200, Time = new DateTimeOffset(new DateTime(2024, 9, 19)).ToUnixTimeSeconds() }
            };

            var result = await _service.StoreTelemetryData(deviceId, telemetryData);

            var storedData = await _context.DeviseTelemetries
                .Where(t => t.DeviceId == deviceId)
                .ToListAsync();

            Assert.Equal(2, storedData.Count);
            Assert.Contains(storedData, t => t.Illum == 150);
            Assert.Contains(storedData, t => t.Illum == 200);
        }

        [Fact]
        public async Task GetDeviceStatistics_ReturnsCorrectStatistics()
        {
            var deviceId = "device1";
            var now = DateTime.UtcNow;
            var telemetryData = new List<DeviceTelemetry>
            {
                new DeviceTelemetry { DeviceId = deviceId, Illum = 200, Timestamp = now.AddDays(-5) },
                new DeviceTelemetry { DeviceId = deviceId, Illum = 300, Timestamp = now.AddDays(-1) }
            };

            _context.DeviseTelemetries.AddRange(telemetryData);
            await _context.SaveChangesAsync();

            var result = await _service.GetDeviceStatistics(deviceId);

            Assert.Equal(2, result.Count);

            var stat1 = result.FirstOrDefault(s => s.Date == now.AddDays(-5).ToString("yyyy-MM-dd"));
            Assert.NotNull(stat1);
            Assert.Equal(200, stat1.MaxIlluminance);

            var stat2 = result.FirstOrDefault(s => s.Date == now.AddDays(-1).ToString("yyyy-MM-dd"));
            Assert.NotNull(stat2);
            Assert.Equal(300, stat2.MaxIlluminance);
        }
    }
}
