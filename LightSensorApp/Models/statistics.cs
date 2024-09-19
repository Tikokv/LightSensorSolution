namespace LightSensorApp.Models
{
    public class statistics
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public DateTime Date { get; set; }
        public double MaxIlluminance { get; set; }
    }
}
