namespace LightSensorSimulator.SimulatorServices
{
    public interface ISensorService
    {
        public void StartSimulation();
        public void StopSimulation();

        void SendTelemetryData(object state);
    }
}
