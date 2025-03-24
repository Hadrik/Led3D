using System.Collections;
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
    public ActionResult<Dictionary<string, string>> GetStripCommands(string driverId, string stripId)
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
}