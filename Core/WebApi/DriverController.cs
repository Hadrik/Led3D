using System.Text.Json;
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
        
        return Ok(driver.GetCommands());
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
    
    [HttpGet("{driverId}/settings")]
    public ActionResult<List<object>> GetDriverSettings(string driverId)
    {
        var driver = _core.Drivers.FirstOrDefault(v => v.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        return Ok(driver.GetSettings());
    }
    
    [HttpPost("{driverId}/settings")]
    public ActionResult SetDriverSetting(string driverId, [FromBody] JsonElement valueElement)
    {
        var driver = _core.Drivers.FirstOrDefault(v => v.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }

        if (valueElement.ValueKind != JsonValueKind.Object)
        {
            return BadRequest("Value must be an object");
        }

        try
        {
            var settings = Parser.ParseJsonObject(valueElement);
            driver.UpdateSettings(settings);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}