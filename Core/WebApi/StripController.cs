using System.Text.Json;
using Led3D_2.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace Led3D_2.WebApi;

[ApiController]
[Route("api/Drivers/{driverId}/[controller]")]
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
    
    [HttpGet("{stripId}/settings")]
    public ActionResult<List<IDictionary<string, object?>>> GetStripCommands(string driverId, string stripId)
    {
        var driver = _core.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        var strip = driver.Strips.FirstOrDefault(s => s.Id == stripId);
        if (strip == null)
        {
            return NotFound("Strip not found");
        }
        
        return strip.GetSettings();
    }
    
    [HttpPost("{stripId}/settings")]
    public ActionResult<object> SetStripSetting(string driverId, string stripId, [FromBody] JsonElement valueElement)
    {
        var driver = _core.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        var strip = driver.Strips.FirstOrDefault(s => s.Id == stripId);
        if (strip == null)
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

