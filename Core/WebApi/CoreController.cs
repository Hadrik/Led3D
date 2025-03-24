using Led3D_2.Core;
using Microsoft.AspNetCore.Mvc;

namespace Led3D_2.WebApi;

[ApiController]
[Route("api/[controller]")]
public class CoreController : ControllerBase
{
    private readonly MainController _mainController = MainController.Instance;
    
    [HttpGet("commands")]
    public ActionResult<List<string>> GetAvailableCommands()
    {
        return _mainController.GetAvailableCommands();
    }
    
    [HttpPost("commands/{command}")]
    public ActionResult<object> ExecuteCommand(string command)
    {
        var result = _mainController.ExecuteCommand(command);
        return Ok(result);
    }
}