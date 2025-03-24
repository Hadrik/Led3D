using Led3D_2.Core;
using Microsoft.AspNetCore.Mvc;

namespace Led3D_2.WebApi;

[ApiController]
[Route("api/[controller]")]
public class DriverController : ControllerBase
{
    private readonly MainController _mainController = MainController.Instance;
    
    [HttpGet("driverIds")]
    public ActionResult<List<string>> GetDriverIds()
    {
        return Ok(_mainController.Drivers.Select(d => d.Id).ToList());
    }
    
    [HttpGet("{driverId}/commands")]
    public ActionResult<List<string>> GetDriverCommands(string driverId)  
    {
        var driver = _mainController.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        return Ok(driver.GetAvailableCommands());
    }
    
    [HttpPost("{driverId}/commands/{command}")]
    public ActionResult<object> ExecuteDriverCommand(string driverId, string command)
    {
        var driver = _mainController.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        var result = driver.ExecuteCommand(command);
        return Ok(result);
    }
}