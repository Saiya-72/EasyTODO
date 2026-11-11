// =============================================================================
// Program.cs — app startup: register services, then configure the HTTP pipeline.
// =============================================================================
// Current state: compiles and runs with dummy controllers. GET /api/health works for real.
//
// Build order that tends to go smoothly:
//   1. Database   — Models/ + Data/AppDbContext.cs, then register it below (STEP 1)
//   2. Categories — easiest controller: plain CRUD, good warm-up
//   3. Todos      — same pattern plus filtering and PATCH
//   4. Auth       — register/login + JWT (STEP 2 below), then add [Authorize]
//                   to the other controllers and swap DummyUserId for the real user id
// =============================================================================

using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Services (dependency injection container) ------------------------------

builder.Services.AddControllers();

// CORS: only needed if the browser calls the backend on a DIFFERENT origin.
// In dev the Vite proxy makes everything same-origin, so this is just a safety net
// for tools or a production setup where the frontend is hosted elsewhere.
const string FrontendCors = "frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});


// --------------------------------------------------------------------------------------
//   The connection string lives in appsettings.json under "ConnectionStrings:Default".
//   docker-compose.yml overrides it with the env var ConnectionStrings__Default
//   (double underscore = nested key).
//
//   AddDbContext registers AppDbContext as "scoped" (one instance per HTTP request),
//   so controllers just ask for it in their constructor.
// --------------------------------------------------------------------------------------

builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// TODO STEP 2 — Authentication (after adding Microsoft.AspNetCore.Authentication.JwtBearer)

var jwt = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<ITokenService, TokenService>();   // see Services/TokenService.cs

var app = builder.Build();

// ---- HTTP pipeline (order matters!) -----------------------------------------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
//   (Fine for a personal project. In real production you'd run migrations as a deploy step.)

if (app.Environment.IsDevelopment())
{
    // Returns detailed exception pages in dev. Leave it on while building.
    app.UseDeveloperExceptionPage();
}

app.UseCors(FrontendCors);

app.UseAuthentication();   // reads the Bearer token and sets HttpContext.User
app.UseAuthorization();    // enforces [Authorize] attributes

app.MapControllers();

// The one real endpoint: lets the frontend confirm it can reach the backend.
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));

app.Run();
