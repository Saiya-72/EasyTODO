// =============================================================================
// TokenService — creates JWTs after a successful login/register.
// =============================================================================
// The interface compiles today; the implementation is a guided TODO because it
// needs the JwtBearer package (which brings in System.IdentityModel.Tokens.Jwt).
//
// A JWT is a signed JSON blob the client sends back on every request:
//   Authorization: Bearer eyJhbGciOi...
// The server verifies the signature with its secret key, so it can trust the
// claims inside (like the user's id) without a database lookup.
// =============================================================================

using TodoApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TodoApi.Services;

public interface ITokenService
{
    string CreateToken(User user);
}

public class TokenService(IConfiguration config) : ITokenService
{
    public string CreateToken(User user)
    {
        var jwt = config.GetSection("Jwt");

        // Claims = facts baked into the token. NameIdentifier is what controllers
        // read back with User.FindFirstValue(ClaimTypes.NameIdentifier).
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwt.GetValue<int>("ExpiresMinutes")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Security notes:
//   - Jwt:Key must be at least 32 characters for HmacSha256, and must NOT be committed
//     for anything real. Use `dotnet user-secrets` locally and an env var in Docker.
//   - Storing the token in localStorage (what the frontend does) is fine for a learning
//     project. Production apps often use an HttpOnly cookie instead to blunt XSS.
