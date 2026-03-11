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

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseInMemoryDatabase("UserDb"));

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

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "super_secret_key_that_needs_to_be_long_enough_for_hs256";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
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
    dbContext.Database.EnsureCreated();
}

// ------------------- Endpoints -------------------

// Auth Endpoint
app.MapPost("/auth/login", async ([Microsoft.AspNetCore.Mvc.FromBody] LoginRequest request, UserDbContext db, IConfiguration config) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);
    if (user == null) return Results.Unauthorized();

    var tokenHandler = new JwtSecurityTokenHandler();
    var secretKey = config["Jwt:Secret"] ?? "super_secret_key_that_needs_to_be_long_enough_for_hs256";
    var tokenKey = Encoding.ASCII.GetBytes(secretKey);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        }),
        Expires = DateTime.UtcNow.AddHours(2),
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
        PasswordHash = req.Password,
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
