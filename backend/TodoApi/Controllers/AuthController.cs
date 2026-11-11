// =============================================================================
// AuthController — /api/auth/*
// =============================================================================
// Frontend counterpart: frontend/src/api/auth.ts
//
// CURRENT STATE: dummy. Every login "succeeds" and returns a fake token.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using TodoApi.Dtos;
using TodoApi.Data;
using TodoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using System.Security.Claims;

namespace TodoApi.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, ITokenService tokens) : ControllerBase
{


    /// <summary>POST /api/auth/register — create an account and log in.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(LoginRequest req)
    {
        var username = req.Username.Trim();

        var userCheck = await db.Users.FromSql($"""
            SELECT * 
            FROM "Users"
            WHERE "Username" = {username}
        """).AnyAsync();

        if(userCheck)
        {
            return Conflict(new ProblemDetails {Title = "Username is taken."});
        }

        var user = new User {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password) };

        await db.Database.ExecuteSqlAsync($"""
            INSERT INTO "Users" ("Id", "Username", "PasswordHash", "CreatedAt")
            VALUES ({user.Id}, {user.Username}, {user.PasswordHash}, {user.CreatedAt})
        """);


        return CreatedAtAction(nameof(Me), new AuthResponse(tokens.CreateToken(user), user.ToDto()));
    }

    /// <summary>POST /api/auth/login — verify credentials, return a token.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await db.Users.FromSql($"""
            SELECT *
            FROM "Users"
            WHERE "Username" = {req.Username.Trim()}
        """).FirstOrDefaultAsync();

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new ProblemDetails { Title = "Invalid username or password." });

        return Ok(new AuthResponse(tokens.CreateToken(user), user.ToDto()));
    }

    /// <summary>GET /api/auth/me — who does this token belong to?</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {   
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FindAsync(id);

        return user is null ? Unauthorized() : Ok(user.ToDto());
    }
}
