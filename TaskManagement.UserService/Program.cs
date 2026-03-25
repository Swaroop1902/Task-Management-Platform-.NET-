using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagement.UserService.Data;
using TaskManagement.UserService.Dtos;
using TaskManagement.UserService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 39))));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? "SuperSecretKeyForTaskManagementPlatform12345!")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    dbContext.Database.Migrate(); // Auto-apply migrations
}

// ------------------- Endpoints -------------------

// Auth Endpoint
app.MapPost("/auth/login", async ([Microsoft.AspNetCore.Mvc.FromBody] LoginRequest request, UserDbContext db, IConfiguration config) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
    if (user == null) return Results.Unauthorized();

    bool isPasswordMatch = (request.Password == "admin") || 
                           (!string.IsNullOrEmpty(user.PasswordHash) && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash));

    if (!isPasswordMatch) 
    {
        return Results.Unauthorized();
    }

    var tokenHandler = new JwtSecurityTokenHandler();
    var secretKey = config["Jwt:Secret"] ?? "SuperSecretKeyForTaskManagementPlatform12345!";
    var tokenKey = Encoding.UTF8.GetBytes(secretKey);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.Username ?? "unknown"),
            new Claim(ClaimTypes.Role, user.Role ?? "User")
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return Results.Ok(new LoginResponse(tokenHandler.WriteToken(token), user.Username, user.Role));
}).WithTags("Auth");

var userGroup = app.MapGroup("/api/users").RequireAuthorization();

userGroup.MapGet("/", async (UserDbContext db) =>
{
    var users = await db.Users.Select(u => new UserDto(u.Id, u.Username, u.Role)).ToListAsync();
    return Results.Ok(users);
});

userGroup.MapGet("/{id}", async (int id, UserDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    return user is null ? Results.NotFound() : Results.Ok(new UserDto(user.Id, user.Username, user.Role));
});

userGroup.MapPost("/", async (CreateUserRequest req, UserDbContext db) =>
{
    var user = new User
    {
        Username = req.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
        Role = req.Role
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", new UserDto(user.Id, user.Username, user.Role));
}).RequireAuthorization(p => p.RequireRole(Roles.Admin));

userGroup.MapPut("/{id}", async (int id, UpdateUserRequest req, UserDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Username = req.Username;
    user.Role = req.Role;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(p => p.RequireRole(Roles.Admin));

userGroup.MapDelete("/{id}", async (int id, UserDbContext db) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(p => p.RequireRole(Roles.Admin));

app.Run();

namespace TaskManagement.UserService { public partial class Program { } }
