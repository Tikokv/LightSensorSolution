namespace LightSensorSimulator.Data
{
    public class SensorSetting
    {
        public string DeviceId { get; set; }
        public string ServerUrl { get; set; }
        public int MeasurementIntervalMinutes { get; set; }
        public int SendIntervalHours { get; set; }
    }
}
