// =============================================================================
// TodosController — /api/todos
// =============================================================================
// Frontend counterpart: frontend/src/api/todos.ts
//
// CURRENT STATE: dummy. Returns fixed data from DummyData.cs; writes are not saved.
//
// THE ONE RULE for every action here: scope every query by the current user.
//   db.TodoItems.Where(t => t.UserId == CurrentUserId && ...)
// Forgetting it on even one endpoint lets users read/edit each other's todos
// just by guessing ids (an "IDOR" bug — one of the most common real-world API flaws).
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TodoApi.Dtos;
using System.Security.Claims;
using TodoApi.Data;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using System.Text.Json.Nodes;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/todos")]
[Authorize]
public class TodosController(AppDbContext db) : ControllerBase
{
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>GET /api/todos?completed=false</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoDto>>> GetAll([FromQuery] bool? completed)
    {

        if(completed == true)
        {
            var todos = await db.TodoItems.FromSql($"""
                SELECT * FROM "TodoItems"
                WHERE "IsCompleted" = {completed} AND "UserId" = {CurrentUserId}
                ORDER BY "CompletedAt" ASC;
            """).ToListAsync();

            return Ok(todos.Select(c => c.ToDto()));
        }

        else if(completed == false){
            var todos = await db.TodoItems.FromSql($"""
                SELECT * FROM "TodoItems"
                WHERE "IsCompleted" = {completed} AND "UserId" = {CurrentUserId}
                ORDER BY "CreatedAt" ASC;
            """).ToListAsync();

            return Ok(todos.Select(c => c.ToDto()));
        }

        else
        {
            var todos = await db.TodoItems.FromSql($"""
                SELECT * FROM "TodoItems"
                WHERE "UserId" = {CurrentUserId}
                ORDER BY "CreatedAt" ASC
            """).ToListAsync();

            return Ok(todos.Select(c => c.ToDto()));
        }
        
    }

    /// <summary>POST /api/todos</summary>
    [HttpPost]
    public async Task<ActionResult<TodoDto>> Create(CreateTodoRequest req)
    {   
        if (req.CategoryId.HasValue)
        {
            var ok = await db.Categories.FromSql($"""
                SELECT * FROM "Categories"
                WHERE "Id" = {req.CategoryId} AND "UserId" = {CurrentUserId}
            """).AnyAsync();
            if (!ok) return BadRequest(new ProblemDetails { Title = "Unknown category." });
        }

        var todo = new TodoItem
        {
            Title = req.Title.Trim(),
            CategoryId = req.CategoryId,
            UserId = CurrentUserId
        };

        await db.Database.ExecuteSqlAsync($"""
            INSERT INTO "TodoItems" ("Id", "Title", "IsCompleted", "CreatedAt", "UserId", "CategoryId")
            VALUES ({todo.Id}, {todo.Title}, {todo.IsCompleted}, {todo.CreatedAt}, {todo.UserId}, {todo.CategoryId})
        """);

        return StatusCode(201, todo.ToDto());
    }

    /// <summary>PATCH /api/todos/{id}</summary>
    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<TodoDto>> Update(Guid id, [FromBody] JsonObject body)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var todo = await db.TodoItems.FromSql($"""
            SELECT *
            FROM "TodoItems"
            WHERE "Id" = {id} AND "UserId" = {CurrentUserId}
        """).FirstOrDefaultAsync();

        if(todo is null) { return NotFound(); }

        if (body.TryGetPropertyValue("title", out var titleNode))      // key was sent
        {
            if (titleNode is not JsonValue tv || !tv.TryGetValue<string>(out var rawTitle))
                return BadRequest(new ProblemDetails { Title = "title must be a string." });

            var title = rawTitle.Trim();
            if (title.Length is 0 or > 200)
                return BadRequest(new ProblemDetails { Title = "title must be 1-200 characters." });

            await db.Database.ExecuteSqlAsync($"""
                UPDATE "TodoItems"
                SET "Title" = {title}
                WHERE "Id" = {id} AND "UserId" = {CurrentUserId};
            """);
        }

        if (body.TryGetPropertyValue("isCompleted", out var doneNode))
        {
            if (doneNode is not JsonValue dv || !dv.TryGetValue<bool>(out var done))
                return BadRequest(new ProblemDetails { Title = "isCompleted must be true or false." });

            if (done != todo.IsCompleted)
            {
                DateTime? completedAt = done ? DateTime.UtcNow : null;
                await db.Database.ExecuteSqlAsync($"""
                    UPDATE "TodoItems"
                    SET "IsCompleted" = {done}, "CompletedAt" = {completedAt}
                    WHERE "Id" = {id} AND "UserId" = {CurrentUserId};
                """);
            }
        }

        // --- categoryId ---
        if (body.TryGetPropertyValue("categoryId", out var catNode))   // key was sent
        {
            Guid? newCategoryId;

            if (catNode is null)
            {
                newCategoryId = null;                                    // explicit null = uncategorize
            }
            else if (catNode is JsonValue v && v.TryGetValue<string>(out var s) && Guid.TryParse(s, out var g))
            {
                newCategoryId = g;

                var owns = await db.Categories.FromSql($"""
                    SELECT * 
                    FROM "Categories"
                    WHERE "Id" = {newCategoryId} AND "UserId" = {CurrentUserId}
                """).AnyAsync();
                if(!owns) return BadRequest(new ProblemDetails { Title = "Unknown category."});
            }
            else
            {
                return BadRequest(new ProblemDetails { Title = "categoryId must be a GUID or null." });
            }

            await db.Database.ExecuteSqlAsync($"""
                UPDATE "TodoItems"
                SET "CategoryId" = {newCategoryId}
                WHERE "Id" = {id} AND "UserId" = {CurrentUserId}
            """);

        }
        // key absent -> do nothing

        await tx.CommitAsync();
        await db.Entry(todo).ReloadAsync();
        return Ok(todo.ToDto());

    }

    /// <summary>DELETE /api/todos/{id}</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await db.Database.ExecuteSqlAsync($"""
            DELETE FROM "TodoItems"
            WHERE "Id" = {id} AND "UserId" = {CurrentUserId}
        """);

        return deleted == 0 ? NotFound() : NoContent();

    }
}
