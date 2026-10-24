using Microsoft.AspNetCore.Mvc;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProjects()
    {
        return Ok("Projecten endpoint werkt");
    }
}