using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Courses.Interfaces;
using SkillHub.Application.Features.Courses.DTOs;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICourseCategoryService _categoryService;

    public CategoryController(ICourseCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseCategoryDto>>> GetCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }
}
