using Microsoft.AspNetCore.Mvc;

namespace Humanitarian.API.Controllers;

public class SystemController : ApiControllerBase
{
    private readonly IWebHostEnvironment _env;

    public SystemController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            name = "Humanitarian Platform API",
            version = "v1",
            environment = _env.EnvironmentName,
            serverTimeUtc = DateTime.UtcNow
        });
    }
}