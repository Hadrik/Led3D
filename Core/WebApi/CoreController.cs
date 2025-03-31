using System.Text.Json;
using Led3D_2.Core;
using Microsoft.AspNetCore.Mvc;

namespace Led3D_2.WebApi;

[ApiController]
[Route("api/[controller]")]
public class CoreController : ControllerBase
{
    private readonly Core.Core _core = Core.Core.Instance;

    [HttpGet("tree")]
    public ActionResult<object> GetSystemTree()
    {
        var tree = new
        {
            Drivers = _core.Drivers.Select(d => new
            {
                Id = d.Id,
                Strips = d.Strips.Select(s => new
                {
                    Id = s.Id
                }).ToList()
            }).ToList(),
            
            Volumes = _core.Volumes.Select(v => new
            {
                Id = v.Id
            }).ToList()
        };
        return Ok(tree);
    }
    
    [HttpGet("commands")]
    public ActionResult<List<string>> GetAvailableCommands()
    {
        return _core.GetCommands();
    }
    
    [HttpPost("commands/{command}")]
    public ActionResult<object> ExecuteCommand(string command)
    {
        var result = _core.ExecuteCommand(command);
        return Ok(result);
    }
    
    [HttpGet("settings")]
    public ActionResult<List<object>> GetVolumeSettings()
    {
        return Ok(_core.GetSettings());
    }
    
    [HttpPost("settings")]
    public ActionResult SetVolumeSetting([FromBody] JsonElement valueElement)
    {
        if (valueElement.ValueKind != JsonValueKind.Object)
        {
            return BadRequest("Value must be an object");
        }

        try
        {
            var settings = Parser.ParseJsonObject(valueElement);
            _core.UpdateSettings(settings);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}