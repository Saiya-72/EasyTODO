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
using TodoApi.Dtos;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/todos")]
// TODO(you): [Authorize]  — on the class, so every action requires a valid token.
public class TodosController : ControllerBase
{
    // TODO(you): primary constructor with AppDbContext:
    //   public class TodosController(AppDbContext db) : ControllerBase

    // TODO(you): once auth works, read the user id from the token's claims:
    //   private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    //   (needs `using System.Security.Claims;`)
    //   Consider a small BaseApiController with this property that all controllers inherit.

    /// <summary>GET /api/todos?completed=false</summary>
    [HttpGet]
    public ActionResult<IEnumerable<TodoDto>> GetAll([FromQuery] bool? completed)
    {
        // TODO(you):
        //   var query = db.TodoItems.Where(t => t.UserId == CurrentUserId);
        //   if (completed is not null) query = query.Where(t => t.IsCompleted == completed);
        //
        //   // Active tab: oldest first. Completed tab: most recently finished first.
        //   query = completed == true
        //       ? query.OrderByDescending(t => t.CompletedAt)
        //       : query.OrderBy(t => t.CreatedAt);
        //
        //   var todos = await query.Select(t => t.ToDto()).ToListAsync();
        //   return Ok(todos);
        //
        //   Note: the .Where/.OrderBy chain is translated to SQL — nothing hits the
        //   DB until ToListAsync(). That's why filtering here beats filtering in C# memory.

        var result = completed is null
            ? DummyData.Todos
            : DummyData.Todos.Where(t => t.IsCompleted == completed).ToList();
        return Ok(result);
    }

    /// <summary>POST /api/todos</summary>
    [HttpPost]
    public ActionResult<TodoDto> Create(CreateTodoRequest req)
    {
        // TODO(you):
        //   1. If req.CategoryId has a value, make sure that category exists AND belongs
        //      to this user, otherwise someone could attach todos to another user's category:
        //        var ok = await db.Categories.AnyAsync(c => c.Id == req.CategoryId && c.UserId == CurrentUserId);
        //        if (!ok) return BadRequest(new ProblemDetails { Title = "Unknown category." });
        //   2. var todo = new TodoItem { Title = req.Title.Trim(), CategoryId = req.CategoryId, UserId = CurrentUserId };
        //   3. db.TodoItems.Add(todo); await db.SaveChangesAsync();
        //   4. return StatusCode(201, todo.ToDto());
        //      (CreatedAtAction(...) is more "correct" REST — it adds a Location header —
        //       but you'd need a GET /api/todos/{id} action to point at.)

        var dummy = new TodoDto(Guid.NewGuid(), req.Title.Trim(), false, req.CategoryId, DateTime.UtcNow, null);
        return StatusCode(201, dummy);
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
