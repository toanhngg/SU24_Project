using BusinessObject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_API.DTO;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        PizzaLabContext context = new PizzaLabContext();
        [HttpGet]
        public IActionResult getCategory()
        {
            var category = context.Categories.ToList();
            return Ok(category);
        }
        [HttpGet("GetCategory/{id}")]
        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var category = await context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            var categoryDto = new CategoryDTO
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description
            };

            return Ok(categoryDto);
        }
        [HttpPost("AddCategory")]
        public async Task<ActionResult<Category>> PostCategory(CategoryDTO categoryDto)
        {
            var category = new Category
            {
                CategoryName = categoryDto.CategoryName,
                Description = categoryDto.Description
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            return CreatedAtAction("GetCategory", new { id = category.Id }, category);
        }
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDTO categoryDto)
        {
            if (id != categoryDto.Id)
            {
                return BadRequest();
            }

            var existingCategory = await context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            var categoryToUpdate = new Category
            {
                Id = id, 
                CategoryName = categoryDto.CategoryName,
                Description = categoryDto.Description
            };

            context.Categories.Attach(categoryToUpdate);

            context.Entry(categoryToUpdate).Property(x => x.CategoryName).IsModified = true;
            context.Entry(categoryToUpdate).Property(x => x.Description).IsModified = true;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(categoryToUpdate);
        }


        [HttpGet("Search/{name}")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> SearchCategoryByName(string name)
        {
            var categories = await context.Categories
                .Where(c => c.CategoryName.Contains(name))
                .Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    Description = c.Description
                })
                .ToListAsync();

            if (categories == null || categories.Count == 0)
            {
                return NotFound();
            }

            return Ok(categories);
        }

        private bool CategoryExists(int id)
        {
            return context.Categories.Any(e => e.Id == id);
        }
    }
}
