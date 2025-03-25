using Led3D_2.Core;
using Microsoft.AspNetCore.Mvc;

namespace Led3D_2.WebApi;

[ApiController]
[Route("api/[controller]")]
public class VolumeController : ControllerBase
{
    private readonly MainController _mainController = MainController.Instance;

    [HttpGet("volumeIds")]
    public ActionResult<List<string>> GetVolume()
    {
        return Ok(_mainController.Volumes.Select(v => v.Id).ToList());
    }

    [HttpGet("{volumeId}/settings")]
    public ActionResult<List<object>> GetVolumeSettings(string volumeId)
    {
        var volume = _mainController.Volumes.FirstOrDefault(v => v.Id == volumeId);
        if (volume == null)
        {
            return NotFound("Volume not found");
        }
        
        return Ok(volume.GetAvailableSettings());
    }
    
    // [HttpPost("{volumeId}/settings/{setting}")]
    // public ActionResult SetVolumeSetting(string volumeId, string setting)
    // {
    //     var volume = _mainController.Volumes.FirstOrDefault(v => v.Id == volumeId);
    //     if (volume == null)
    //     {
    //         return NotFound("Volume not found");
    //     }
    //     
    //     return Ok(volume.GetAvailableSettings());
    // }
}