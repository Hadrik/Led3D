using Led3D_2.Core;
using Led3D_2.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Led3D_2.WebApi;

[ApiController]
[Route("api/[controller]")]
public class DriverController : ControllerBase
{
    private readonly Core.Core _core = Core.Core.Instance;
    
    [HttpGet("driverIds")]
    public ActionResult<List<string>> GetDriverIds()
    {
        return Ok(_core.Drivers.Select(d => d.Id).ToList());
    }
    
    [HttpGet("{driverId}/commands")]
    public ActionResult<List<string>> GetDriverCommands(string driverId)  
    {
        var driver = _core.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        return Ok(driver.GetAvailableCommands());
    }
    
    [HttpPost("{driverId}/commands/{command}")]
    public ActionResult<object> ExecuteDriverCommand(string driverId, string command)
    {
        if (_core.Drivers.FirstOrDefault(d => d.Id == driverId) is not ICommandProvider driver)
        {
            return NotFound("Driver not found");
        }

        try
        {
            var result = driver.ExecuteCommand(command);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}