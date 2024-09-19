using LightSensorApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LightSensorApp.Db
{
    public class LightSensorContext : DbContext
    {
        public LightSensorContext(DbContextOptions<LightSensorContext> options)
        : base(options)
        {
        }

        public DbSet<DeviceTelemetry> DeviseTelemetries { get; set; }
    }
}
