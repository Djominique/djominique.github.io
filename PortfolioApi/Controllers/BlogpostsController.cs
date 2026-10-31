using Microsoft.AspNetCore.Mvc;
using PortfolioApi.DTOs;
using PortfolioApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace PortfolioApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogpostsController : ControllerBase
{
    private readonly BlogpostService _blogpostService;

    public BlogpostsController(BlogpostService blogpostService)
    {
        _blogpostService = blogpostService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBlogposts()
    {
        var blogposts = await _blogpostService.GetAllAsync();
        return Ok(blogposts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBlogpost(int id)
    {
        var blogpost = await _blogpostService.GetByIdAsync(id);

        if (blogpost == null)
        {
            return NotFound();
        }

        return Ok(blogpost);
    }

    [Authorize(Policy = "CanEditPortfolio")]
[HttpPost]
    public async Task<IActionResult> CreateBlogpost(CreateBlogpostDto dto)
    {
        var blogpost = await _blogpostService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetBlogpost),
            new { id = blogpost.Id },
            blogpost
        );
    }

    [Authorize(Policy = "CanEditPortfolio")]
[HttpPut("{id}")]
    public async Task<IActionResult> UpdateBlogpost(int id, UpdateBlogpostDto dto)
    {
        var gelukt = await _blogpostService.UpdateAsync(id, dto);

        if (!gelukt)
        {
            return NotFound();
        }

        return NoContent();
    }

    [Authorize(Policy = "CanEditPortfolio")]
[HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBlogpost(int id)
    {
        var gelukt = await _blogpostService.DeleteAsync(id);

        if (!gelukt)
        {
            return NotFound();
        }

        return NoContent();
    }
}