using System.Text.Json;
using Led3D_2.Core;
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

    [HttpGet("{volumeId}/settings")]
    public ActionResult<List<object>> GetVolumeSettings(string volumeId)
    {
        var volume = _core.Volumes.FirstOrDefault(v => v.Id == volumeId);
        if (volume == null)
        {
            return NotFound("Volume not found");
        }
        
        return Ok(volume.GetSettings());
    }
    
    [HttpPost("{volumeId}/settings")]
    public ActionResult SetVolumeSetting(string volumeId, [FromBody] JsonElement valueElement)
    {
        var volume = _core.Volumes.FirstOrDefault(v => v.Id == volumeId);
        if (volume == null)
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