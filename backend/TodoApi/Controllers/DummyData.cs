// =============================================================================
// DummyData — fixed responses so the frontend can talk to the backend before
// the database exists. DELETE THIS FILE once every controller uses AppDbContext;
// the compiler will then point you at anything still using it.
// =============================================================================

using TodoApi.Dtos;

namespace TodoApi.Controllers;

internal static class DummyData
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid WorkId = Guid.Parse("22222222-0000-0000-0000-000000000001");
    public static readonly Guid SchoolId = Guid.Parse("22222222-0000-0000-0000-000000000002");

    public static readonly List<CategoryDto> Categories =
    [
        new(WorkId, "Work (from backend)"),
        new(SchoolId, "School (from backend)"),
    ];

    public static readonly List<TodoDto> Todos =
    [
        new(Guid.Parse("33333333-0000-0000-0000-000000000001"), "Dummy todo from the C# backend", false, WorkId, DateTime.UtcNow, null),
        new(Guid.Parse("33333333-0000-0000-0000-000000000002"), "Another dummy todo", false, null, DateTime.UtcNow, null),
        new(Guid.Parse("33333333-0000-0000-0000-000000000003"), "A completed dummy todo", true, SchoolId, DateTime.UtcNow, DateTime.UtcNow),
    ];
}
