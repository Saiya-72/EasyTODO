// =============================================================================
// CategoriesController — /api/categories
// =============================================================================
// Frontend counterpart: frontend/src/api/categories.ts
//
// CURRENT STATE: dummy. Good first controller to implement for real — it's the
// simplest CRUD in the app and exercises everything (DbContext, user scoping,
// validation, 404/409 responses).
//
// APPROACH: hand-written SQL (PostgreSQL), run through EF Core. No LINQ queries.
//   db.Categories.FromSql($"SELECT ...")   -> rows mapped to Category entities
//   db.Database.ExecuteSqlAsync($"...")    -> INSERT/UPDATE/DELETE, returns rows affected
//   (both need `using Microsoft.EntityFrameworkCore;`)
//
// PARAMETERS: inside $"..." every {value} becomes a SQL parameter (@p0, @p1...),
// NOT pasted text — that's what prevents SQL injection. Never use FromSqlRaw /
// ExecuteSqlRawAsync with a string built by concatenation.
//
// QUOTING: EF created PascalCase names. Postgres lowercases unquoted identifiers,
// so you must write "Categories", "UserId", "Name" in double quotes. Use a C#
// raw string ($"""...""") so the quotes don't need escaping.
//
// PRACTICE FIRST in psql, then paste into C#:
//   docker compose exec db psql -U todo -d todo
//   \d "Categories"      -- columns, indexes, foreign keys
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TodoApi.Dtos;
using TodoApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    // and the same CurrentUserId property as TodosController.
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>GET /api/categories</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await db.Categories
            .FromSql($"""
                SELECT * FROM "Categories"
                WHERE "UserId" = {CurrentUserId}
                ORDER BY "Name"
                """)
            .ToListAsync();

        return Ok(categories.Select(c => c.ToDto()));
    }

    /// <summary>POST /api/categories</summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest req)
    {
        var name = req.Name.Trim();
        var id = Guid.NewGuid();

        //   Why ON CONFLICT instead of "SELECT first, then INSERT"? Two requests at the same
        //   instant could both pass the SELECT check. Letting the unique index on
        //   ("UserId","Name") decide, in one statement, has no race. ExecuteSqlAsync returns
        //   rows affected, so 0 means the name was taken.
        //   Alternative to know: drop ON CONFLICT and catch Npgsql.PostgresException with
        //   SqlState == PostgresErrorCodes.UniqueViolation ("23505").

        var inserted = await db.Database.ExecuteSqlAsync($"""
            INSERT INTO "Categories" ("Id", "Name", "UserId")
            VALUES ({id}, {name}, {CurrentUserId})
            ON CONFLICT ("UserId", "Name") DO NOTHING
        """);
        if (inserted == 0)
           return Conflict(new ProblemDetails { Title = "A category with that name already exists." });

        return StatusCode(201, new CategoryDto(id, name));
    }

    /// <summary>DELETE /api/categories/{id}</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        //   The "UserId" check matters: without it, anyone who guesses an id could delete
        //   another user's category. Returning 404 (not 403) also hides that it exists.
        //
        //   Todos in this category get "CategoryId" = NULL automatically — the migration
        //   created the foreign key with ON DELETE SET NULL, so Postgres itself does it.
        //   Verify in psql with: \d "TodoItems"

        var deleted = await db.Database.ExecuteSqlAsync($"""
            DELETE FROM "Categories"
            WHERE "Id" = {id} AND "UserId" = {CurrentUserId}
            """);
        if (deleted == 0) return NotFound();
        return NoContent();
    }
}
