using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Domain.Entities; 
using Microsoft.AspNetCore.Mvc;

namespace Knowledge_Repository.Controllers
{
 
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        public async Task<ActionResult<List<Category>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);

            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpGet("byname/{name}")]
        public async Task<ActionResult<Category>> GetCategoryByName(string name)
        {
            var category = await _categoryService.GetCategoryByNameAsync(name);

            if (category == null)
                return NotFound();
            return Ok(category);
        }
        [HttpPost]
        public async Task<ActionResult> CreateCategory([FromBody] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                return BadRequest("Category name is required.");
            if (category.DomainId == Guid.Empty)
                return BadRequest("Domain ID is required.");

            await _categoryService.AddCategoryAsync(category);
            return Ok(new { message = "Category added successfully." });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] Category category)
        {
            var success = await _categoryService.UpdateCategoryAsync(id, category);
            if (!success) return NotFound();
            return Ok(new { message = "Category updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {
            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Category deleted successfully." });
        }

    }
}
