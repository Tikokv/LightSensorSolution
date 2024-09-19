using System.ComponentModel.DataAnnotations;

namespace LightSensorApp.Models
{
    public class DeviceTelemetry
    {
        [Key]
        public int Id { get; set; }
        public double Illum { get; set; }
        public DateTime Timestamp { get; set; }
        public string DeviceId { get; set; }
    }
}
