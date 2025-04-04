using System.Text.Json;
using Led3D_2.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Led3D_2.WebApi;

[ApiController]
[Route("api/Driver/{driverId}/[controller]")]
public class StripController : ControllerBase
{
    private readonly Core.Core _core = Core.Core.Instance;

    [HttpGet("stripIds")]
    public ActionResult<List<string>> GetStripIds(string driverId)
    {
        var driver = _core.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        return driver.Strips.Select(s => s.Id).ToList();
    }
    
    [HttpGet("{stripId}/commands")]
    public ActionResult<List<string>> GetAvailableCommands(string driverId, string stripId)
    {
        var driver = _core.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        if (driver.Strips.FirstOrDefault(s => s.Id == stripId) is not ICommandProvider strip)
        {
            return NotFound("Strip not found");
        }
        
        return strip.GetCommands();
    }
    
    [HttpPost("{stripId}/commands/{command}")]
    public ActionResult<object> ExecuteCommand(string driverId, string stripId, string command)
    {
        var driver = _core.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        if (driver.Strips.FirstOrDefault(s => s.Id == stripId) is not ICommandProvider strip)
        {
            return NotFound("Strip not found");
        }

        try
        {
            var result = strip.ExecuteCommand(command);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpGet("{stripId}/settings")]
    public ActionResult<List<IDictionary<string, object?>>> GetSettings(string driverId, string stripId)
    {
        var driver = _core.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        if (driver.Strips.FirstOrDefault(s => s.Id == stripId) is not ISettingsProvider strip)
        {
            return NotFound("Strip not found");
        }
        
        return strip.GetSettings();
    }
    
    [HttpPost("{stripId}/settings")]
    public ActionResult<object> SetSettings(string driverId, string stripId, [FromBody] JsonElement valueElement)
    {
        var driver = _core.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        if (driver.Strips.FirstOrDefault(s => s.Id == stripId) is not ISettingsProvider strip)
        {
            return NotFound("Strip not found");
        }

        if (valueElement.ValueKind != JsonValueKind.Object)
        {
            return BadRequest("Value must be an object");
        }
        
        try
        {
            var value = Parser.ParseJsonObject(valueElement);
            strip.UpdateSettings(value);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}

