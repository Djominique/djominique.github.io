using Microsoft.AspNetCore.Mvc;
using PortfolioApi.DTOs;
using PortfolioApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var projects = await _projectService.GetAllAsync();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(int id)
    {
        var project = await _projectService.GetByIdAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        return Ok(project);
    }

    [Authorize(Policy = "CanEditPortfolio")]
[HttpPost]
public async Task<IActionResult> CreateProject(CreateProjectDto dto)
    {
        var project = await _projectService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetProject),
            new { id = project.Id },
            project
        );
    }

    [Authorize(Policy = "CanEditPortfolio")]
[HttpPut("{id}")]
public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto dto)
    {
        var gelukt = await _projectService.UpdateAsync(id, dto);

        if (!gelukt)
        {
            return NotFound();
        }

        return NoContent();
    }

   [Authorize(Policy = "CanEditPortfolio")]
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteProject(int id)
    {
        var gelukt = await _projectService.DeleteAsync(id);

        if (!gelukt)
        {
            return NotFound();
        }

        return NoContent();
    }
}