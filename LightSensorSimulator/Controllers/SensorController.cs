using LightSensorSimulator.SimulatorServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LightSensorSimulator.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly SensorService _simulationService;

        public SensorController(SensorService simulationService)
        {
            _simulationService = simulationService;
        }

        [HttpPost("start")]
        public IActionResult StartSimulation()
        {
            _simulationService.StartSimulation();
            return Ok("Simulation started.");
        }

        
        [HttpPost("stop")]
        public IActionResult StopSimulation()
        {
            _simulationService.StopSimulation();
            return Ok("Simulation stopped.");
        }
    }
}