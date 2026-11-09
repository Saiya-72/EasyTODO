// =============================================================================
// Entities — the shapes stored in the database.
// =============================================================================
// These are NOT what the API returns. Controllers map entities -> DTOs (Dtos/),
// so you never accidentally send PasswordHash to the browser, and the DB schema
// can change without breaking the frontend.
//
// Relationships:
//   User 1 ── * Category
//   User 1 ── * TodoItem
//   Category 1 ── * TodoItem   (optional: TodoItem.CategoryId can be null)
//
// EF Core figures most of this out from naming conventions:
//   - a property called "Id" becomes the primary key
//   - "UserId" + a "User" navigation property becomes a foreign key
// Anything it can't infer (unique indexes, delete behavior) goes in AppDbContext.OnModelCreating.
// =============================================================================

namespace TodoApi.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Username { get; set; }

    // NEVER store the plain password. Hash with BCrypt:
    //   PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
    //   bool ok      = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    public required string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties (EF Core populates these when you .Include() them)
    public List<TodoItem> Todos { get; set; } = [];
    public List<Category> Categories { get; set; } = [];
}

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public List<TodoItem> Todos { get; set; } = [];
}

// Named TodoItem rather than Todo to avoid clashing with the namespace-ish word "Todo"
// and to read clearly in LINQ (db.TodoItems.Where(...)).
public class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Title { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Set when IsCompleted flips to true, cleared when it flips back.
    // Lets the Completed tab sort by "most recently finished".
    public DateTime? CompletedAt { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid? CategoryId { get; set; } // nullable = "Uncategorized"
    public Category? Category { get; set; }
}
