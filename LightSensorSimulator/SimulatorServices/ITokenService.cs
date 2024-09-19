namespace LightSensorSimulator.SimulatorServices
{
    public interface ITokenService
    {
        Task<string> AuthenticateAsync(string username, string password);
    }
}
