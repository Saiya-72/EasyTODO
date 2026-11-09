// =============================================================================
// CategoriesController — /api/categories
// =============================================================================
// Frontend counterpart: frontend/src/api/categories.ts
//
// CURRENT STATE: dummy. Good first controller to implement for real — it's the
// simplest CRUD in the app and exercises everything (DbContext, user scoping,
// validation, 404/409 responses).
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TodoApi.Dtos;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/categories")]
// TODO(you): [Authorize]
public class CategoriesController : ControllerBase
{
    // TODO(you): public class CategoriesController(AppDbContext db) : ControllerBase
    // and the same CurrentUserId property as TodosController.

    /// <summary>GET /api/categories</summary>
    [HttpGet]
    public ActionResult<IEnumerable<CategoryDto>> GetAll()
    {
        // TODO(you):
        //   var categories = await db.Categories
        //       .Where(c => c.UserId == CurrentUserId)
        //       .OrderBy(c => c.Name)
        //       .Select(c => c.ToDto())
        //       .ToListAsync();
        //   return Ok(categories);

        return Ok(DummyData.Categories);
    }

    /// <summary>POST /api/categories</summary>
    [HttpPost]
    public ActionResult<CategoryDto> Create(CreateCategoryRequest req)
    {
        // TODO(you):
        //   var name = req.Name.Trim();
        //   var exists = await db.Categories.AnyAsync(c => c.UserId == CurrentUserId && c.Name == name);
        //   if (exists) return Conflict(new ProblemDetails { Title = "A category with that name already exists." });
        //
        //   var category = new Category { Name = name, UserId = CurrentUserId };
        //   db.Categories.Add(category);
        //   await db.SaveChangesAsync();
        //   return StatusCode(201, category.ToDto());
        //
        //   The AnyAsync check gives a friendly error, but two requests at the same instant
        //   could both pass it. The unique index in AppDbContext is the real guarantee —
        //   catch DbUpdateException around SaveChangesAsync and return Conflict there too.

        return StatusCode(201, new CategoryDto(Guid.NewGuid(), req.Name.Trim()));
    }

    /// <summary>DELETE /api/categories/{id}</summary>
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        // TODO(you):
        //   var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == id && c.UserId == CurrentUserId);
        //   if (category is null) return NotFound();
        //   db.Categories.Remove(category);
        //   await db.SaveChangesAsync();
        //   return NoContent();
        //
        //   Todos in this category get CategoryId = NULL automatically thanks to
        //   OnDelete(DeleteBehavior.SetNull) in AppDbContext.
        //   SQLite caveat: EF only applies SetNull to todos it has loaded. To be safe,
        //   load them first:  .Include(c => c.Todos)  on the query above.

        _ = id;
        return NoContent();
    }
}
