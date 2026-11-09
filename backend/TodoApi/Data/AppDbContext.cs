// =============================================================================
// AppDbContext — Entity Framework Core's gateway to the database.
// =============================================================================
// Commented out because it needs an EF Core package to compile (see TodoApi.csproj).
//
// TO ENABLE:
//   1. dotnet add package Microsoft.EntityFrameworkCore.Sqlite      (or Npgsql...PostgreSQL)
//      dotnet add package Microsoft.EntityFrameworkCore.Design
//   2. Uncomment the class below.
//   3. Register it in Program.cs (TODO STEP 1).
//   4. Create the first migration (from backend/TodoApi/):
//        dotnet ef migrations add InitialCreate
//      This generates a Migrations/ folder with C# that creates the tables. Commit it.
//   5. Apply it:
//        dotnet ef database update
//      ...or let Program.cs call db.Database.Migrate() on startup (TODO STEP 1b).
//
// Every time you change an entity in Models/, repeat steps 4–5 with a new migration name.
// =============================================================================

// using Microsoft.EntityFrameworkCore;
// using TodoApi.Models;
//
// namespace TodoApi.Data;
//
// public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
// {
//     // Each DbSet becomes a table. You query them with LINQ:
//     //   await db.TodoItems.Where(t => t.UserId == userId).ToListAsync();
//     public DbSet<User> Users => Set<User>();
//     public DbSet<Category> Categories => Set<Category>();
//     public DbSet<TodoItem> TodoItems => Set<TodoItem>();
//
//     protected override void OnModelCreating(ModelBuilder model)
//     {
//         // Usernames must be unique (login looks users up by name).
//         model.Entity<User>()
//             .HasIndex(u => u.Username)
//             .IsUnique();
//
//         // A user can't have two categories with the same name.
//         // Catch DbUpdateException in the controller and return 409 Conflict.
//         model.Entity<Category>()
//             .HasIndex(c => new { c.UserId, c.Name })
//             .IsUnique();
//
//         // Deleting a category should NOT delete its todos — just un-categorize them.
//         model.Entity<TodoItem>()
//             .HasOne(t => t.Category)
//             .WithMany(c => c.Todos)
//             .HasForeignKey(t => t.CategoryId)
//             .OnDelete(DeleteBehavior.SetNull);
//
//         // Deleting a user deletes everything they own.
//         model.Entity<TodoItem>()
//             .HasOne(t => t.User)
//             .WithMany(u => u.Todos)
//             .HasForeignKey(t => t.UserId)
//             .OnDelete(DeleteBehavior.Cascade);
//
//         // Speeds up the most common query: "this user's active/completed todos".
//         model.Entity<TodoItem>()
//             .HasIndex(t => new { t.UserId, t.IsCompleted });
//
//         // Length limits -> real column constraints (keep in sync with the DTO validation).
//         model.Entity<User>().Property(u => u.Username).HasMaxLength(50);
//         model.Entity<Category>().Property(c => c.Name).HasMaxLength(50);
//         model.Entity<TodoItem>().Property(t => t.Title).HasMaxLength(200);
//     }
// }
