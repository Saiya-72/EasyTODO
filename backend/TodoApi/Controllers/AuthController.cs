// =============================================================================
// AuthController — /api/auth/*
// =============================================================================
// Frontend counterpart: frontend/src/api/auth.ts
//
// CURRENT STATE: dummy. Every login "succeeds" and returns a fake token.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TodoApi.Dtos;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // TODO(you): inject dependencies through the constructor (primary constructor syntax):
    //   public class AuthController(AppDbContext db, ITokenService tokens) : ControllerBase
    // ASP.NET Core's DI container supplies them per request.

    /// <summary>POST /api/auth/register — create an account and log in.</summary>
    [HttpPost("register")]
    public ActionResult<AuthResponse> Register(LoginRequest req)
    {
        // TODO(you):
        //   1. Normalize: var username = req.Username.Trim();
        //   2. If await db.Users.AnyAsync(u => u.Username == username)
        //        -> return Conflict(new ProblemDetails { Title = "Username is taken." });   // 409
        //   3. var user = new User {
        //          Username = username,
        //          PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password) };
        //   4. db.Users.Add(user); await db.SaveChangesAsync();
        //   5. Optional: seed a default category or two for new users.
        //   6. return CreatedAtAction(nameof(Me), new AuthResponse(tokens.CreateToken(user), user.ToDto()));
        //
        //   Make the method `async Task<ActionResult<AuthResponse>>` once you await things.

        return StatusCode(201, DummyAuth(req.Username));
    }

    /// <summary>POST /api/auth/login — verify credentials, return a token.</summary>
    [HttpPost("login")]
    public ActionResult<AuthResponse> Login(LoginRequest req)
    {
        // TODO(you):
        //   1. var user = await db.Users.SingleOrDefaultAsync(u => u.Username == req.Username.Trim());
        //   2. if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        //          return Unauthorized(new ProblemDetails { Title = "Invalid username or password." });
        //      Use the SAME message for "no such user" and "wrong password" so attackers
        //      can't use the login form to discover which usernames exist.
        //   3. return Ok(new AuthResponse(tokens.CreateToken(user), user.ToDto()));

        return Ok(DummyAuth(req.Username));
    }

    /// <summary>GET /api/auth/me — who does this token belong to?</summary>
    // TODO(you): add [Authorize] here once JWT auth is configured in Program.cs.
    [HttpGet("me")]
    public ActionResult<UserDto> Me()
    {
        // TODO(you):
        //   [Authorize] guarantees a valid token, so the claims are trustworthy:
        //     var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        //     var user = await db.Users.FindAsync(id);
        //     return user is null ? Unauthorized() : Ok(user.ToDto());
        //   (The user could have been deleted after the token was issued, hence the null check.)

        return Ok(new UserDto(DummyData.UserId, "demo"));
    }

    private static AuthResponse DummyAuth(string username) =>
        new("dummy-token", new UserDto(DummyData.UserId, username.Trim()));
}
