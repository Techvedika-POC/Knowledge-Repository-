using KnowLedger_Synaptix.Dtos;
using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowLedger_Synaptix.Controllers
{
    /// <summary>
    /// Manages category-related operations such as retrieving all categories,
    /// fetching by ID, or searching by name.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Retrieves all available categories.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(Guid id)
        {
            // Fetch category by unique ID
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpGet("byname/{name}")]
        public async Task<ActionResult<Category>> GetCategoryByName(string name)
        {
            // Fetch category by name (case-insensitive)
            var category = await _categoryService.GetCategoryByNameAsync(name);

            if (category == null)
                return NotFound();
            return Ok(category);
        }
    }
}
