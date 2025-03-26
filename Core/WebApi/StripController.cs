using System.Text.Json;
using Led3D_2.Core;
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
            var value = ParseJsonObject(valueElement);
            strip.UpdateSettings(value);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    private object ParseJsonElement(JsonElement element)
    {
        object result = element.ValueKind switch
        {
            JsonValueKind.Object => ParseJsonObject(element),
            JsonValueKind.String => element.GetString()!,
            JsonValueKind.Number => element.TryGetInt32(out int intVal) ? (object)intVal : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => throw new ArgumentException($"Unsupported JSON value kind: {element.ValueKind}")
        };
        return result;
    }

    private Dictionary<string, object> ParseJsonObject(JsonElement element)
    {
        var result = new Dictionary<string, object>();
        foreach (var property in element.EnumerateObject())
        {
            result[property.Name] = ParseJsonElement(property.Value);
        }
        return result;
    }
}

