// =============================================================================
// DTOs (Data Transfer Objects) — exactly what goes over the wire.
// =============================================================================
// Must match frontend/src/types.ts. ASP.NET Core's JSON serializer converts
// PascalCase to camelCase automatically (Title -> "title"), and Guid -> string.
//
// Records give you immutability, value equality and a one-line declaration.
// Validation attributes ([Required], [StringLength]) are checked automatically
// by [ApiController]: a bad request gets a 400 ProblemDetails response before
// your action method even runs.
// =============================================================================

using System.ComponentModel.DataAnnotations;
using TodoApi.Models;

namespace TodoApi.Dtos;

// ---- Auth --------------------------------------------------------------------

public record LoginRequest(
    [Required, StringLength(50, MinimumLength = 3)] string Username,
    [Required, StringLength(100, MinimumLength = 6)] string Password);

public record UserDto(Guid Id, string Username);

public record AuthResponse(string Token, UserDto User);

// ---- Categories --------------------------------------------------------------

public record CategoryDto(Guid Id, string Name);

public record CreateCategoryRequest([Required, StringLength(50, MinimumLength = 1)] string Name);

// ---- Todos -------------------------------------------------------------------

public record TodoDto(
    Guid Id,
    string Title,
    bool IsCompleted,
    Guid? CategoryId,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public record CreateTodoRequest(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    Guid? CategoryId);

// PATCH body: every field optional; null means "don't change this".
//
// GOTCHA: CategoryId is ambiguous. The frontend sends { "categoryId": null } to mean
// "move to Uncategorized", but a missing field ALSO deserializes to null here.
// Accept the body as System.Text.Json.Nodes.JsonObject in the controller and
// check body.ContainsKey("categoryId") to tell "absent" from "null".

public record UpdateTodoRequest(
    [StringLength(200, MinimumLength = 1)] string? Title,
    bool? IsCompleted,
    Guid? CategoryId);

// ---- Mapping helpers ---------------------------------------------------------
// Extension methods keep entity -> DTO conversion in one place: todo.ToDto()
// (AutoMapper/Mapster exist, but for 3 types hand-written is clearer.)

public static class DtoMappings
{
    public static UserDto ToDto(this User u) => new(u.Id, u.Username);

    public static CategoryDto ToDto(this Category c) => new(c.Id, c.Name);

    public static TodoDto ToDto(this TodoItem t) =>
        new(t.Id, t.Title, t.IsCompleted, t.CategoryId, t.CreatedAt, t.CompletedAt);
}
