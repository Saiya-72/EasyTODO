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
    public ActionResult<TodoDto> Update(Guid id, UpdateTodoRequest req)
    {
        // TODO(you):
        //   1. Load it, scoped to the user:
        //        var todo = await db.TodoItems.SingleOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        //        if (todo is null) return NotFound();
        //   2. Apply only the fields that were sent:
        //        if (req.Title is not null) todo.Title = req.Title.Trim();
        //        if (req.IsCompleted is bool done && done != todo.IsCompleted)
        //        {
        //            todo.IsCompleted = done;
        //            todo.CompletedAt = done ? DateTime.UtcNow : null;
        //        }
        //        // CategoryId: see the GOTCHA comment on UpdateTodoRequest in Dtos.cs.
        //        // Validate ownership of the new category the same way Create does.
        //   3. await db.SaveChangesAsync();
        //      EF Core tracks changes on loaded entities, so no explicit Update() call is needed.
        //   4. return Ok(todo.ToDto());

        var existing = DummyData.Todos.FirstOrDefault(t => t.Id == id);
        if (existing is null) return NotFound();

        var updated = existing with
        {
            Title = req.Title ?? existing.Title,
            IsCompleted = req.IsCompleted ?? existing.IsCompleted,
            CategoryId = req.CategoryId ?? existing.CategoryId,
            CompletedAt = req.IsCompleted switch
            {
                true => DateTime.UtcNow,
                false => null,
                null => existing.CompletedAt,
            },
        };
        return Ok(updated); // not persisted — dummy
    }

    /// <summary>DELETE /api/todos/{id}</summary>
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        // TODO(you):
        //   var todo = await db.TodoItems.SingleOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        //   if (todo is null) return NotFound();
        //   db.TodoItems.Remove(todo);
        //   await db.SaveChangesAsync();
        //   return NoContent();   // 204
        //
        //   (EF Core 7+ shortcut, one SQL statement, no load:
        //    var rows = await db.TodoItems.Where(t => t.Id == id && t.UserId == CurrentUserId).ExecuteDeleteAsync();
        //    return rows == 0 ? NotFound() : NoContent();)

        _ = id;
        return NoContent();
    }
}
