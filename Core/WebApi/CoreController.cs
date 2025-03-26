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
            drivers = _core.Drivers.Select(d => new
            {
                Id = d.Id,
                Strips = d.Strips.Select(s => new
                {
                    Id = s.Id
                }).ToList()
            }).ToList()
        };
        return Ok(tree);
    }
    
    [HttpGet("commands")]
    public ActionResult<List<string>> GetAvailableCommands()
    {
        return _core.GetAvailableCommands();
    }
    
    [HttpPost("commands/{command}")]
    public ActionResult<object> ExecuteCommand(string command)
    {
        var result = _core.ExecuteCommand(command);
        return Ok(result);
    }
}