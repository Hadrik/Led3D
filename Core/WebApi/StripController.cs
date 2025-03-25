using System.Text.Json;
using Led3D_2.Core;
using Microsoft.AspNetCore.Mvc;

namespace Led3D_2.WebApi;

[ApiController]
[Route("api/Drivers/{driverId}/[controller]")]
public class StripController : ControllerBase
{
    private readonly MainController _mainController = MainController.Instance;

    [HttpGet("stripIds")]
    public ActionResult<List<string>> GetStripIds(string driverId)
    {
        var driver = _mainController.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        return driver.Strips.Select(s => s.Id).ToList();
    }
    
    [HttpGet("{stripId}/settings")]
    public ActionResult<List<IDictionary<string, object?>>> GetStripCommands(string driverId, string stripId)
    {
        var driver = _mainController.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        var strip = driver.Strips.FirstOrDefault(s => s.Id == stripId);
        if (strip == null)
        {
            return NotFound("Strip not found");
        }
        
        return strip.GetAvailableSettings();
    }
    
    [HttpPost("{stripId}/settings/{setting}")]
    public ActionResult<object> SetStripSetting(string driverId, string stripId, string setting, [FromBody] JsonElement valueElement)
    {
        var driver = _mainController.Drivers.FirstOrDefault(d => d.Id == driverId);
        if (driver == null)
        {
            return NotFound("Driver not found");
        }
        
        var strip = driver.Strips.FirstOrDefault(s => s.Id == stripId);
        if (strip == null)
        {
            return NotFound("Strip not found");
        }
        
        try
        {
            var value = valueElement.ValueKind switch
            {
                JsonValueKind.String => valueElement.GetString()!,
                JsonValueKind.Number => valueElement.TryGetInt32(out var intVal) ? intVal : valueElement.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null!,
                _ => JsonSerializer.Deserialize<object>(valueElement.GetRawText())!
            };
            strip.UpdateSetting(setting, value);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}