using System.Text.Json;
using Led3D_2.Utility;
using Microsoft.AspNetCore.Mvc;

namespace Led3D_2.WebApi;

[ApiController]
[Route("api/[controller]")]
public class VolumeController : ControllerBase
{
    private readonly Core.Core _core = Core.Core.Instance;

    [HttpGet("volumeIds")]
    public ActionResult<List<string>> GetVolume()
    {
        return Ok(_core.Volumes.Select(v => v.Id).ToList());
    }
    
    [HttpGet("{volumeId}/commands")]
    public ActionResult<List<string>> GetCommands(string volumeId)  
    {
        if (_core.Volumes.FirstOrDefault(v => v.Id == volumeId) is not ICommandProvider volume)
        {
            return NotFound("Volume not found");
        }
        
        return Ok(volume.GetCommands());
    }
    
    [HttpPost("{volumeId}/commands/{command}")]
    public ActionResult<object> ExecuteCommand(string volumeId, string command)
    {
        if (_core.Volumes.FirstOrDefault(v => v.Id == volumeId) is not ICommandProvider volume)
        {
            return NotFound("Volume not found");
        }

        try
        {
            var result = volume.ExecuteCommand(command);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{volumeId}/settings")]
    public ActionResult<List<object>> GetSettings(string volumeId)
    {
        if (_core.Volumes.FirstOrDefault(v => v.Id == volumeId) is not ISettingsProvider volume)
        {
            return NotFound("Volume not found");
        }
        
        return Ok(volume.GetSettings());
    }
    
    [HttpPost("{volumeId}/settings")]
    public ActionResult SetSetting(string volumeId, [FromBody] JsonElement valueElement)
    {
        if (_core.Volumes.FirstOrDefault(v => v.Id == volumeId) is not ISettingsProvider volume)
        {
            return NotFound("Volume not found");
        }

        if (valueElement.ValueKind != JsonValueKind.Object)
        {
            return BadRequest("Value must be an object");
        }

        try
        {
            var settings = Parser.ParseJsonObject(valueElement);
            volume.UpdateSettings(settings);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}