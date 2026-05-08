using Crm.Identity.Application.DTOs;
using Crm.Identity.Application.Interfaces;
using Crm.Identity.Infrastructure;
using Crm.Identity.Infrastructure.Persistence;
using Crm.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors();
app.UseAuthentication();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Identity" }));

app.MapPost("/auth/register", async Task<Results<Ok<ApiResponse<TokenResponseDto>>, BadRequest<ApiResponse<TokenResponseDto>>>> (
    RegisterUserDto dto, HttpContext http, CancellationToken ct) =>
{
    var userService = http.RequestServices.GetRequiredService<IUserService>();
    try
    {
        var result = await userService.RegisterAsync(dto, ct);
        return TypedResults.Ok(ApiResponse<TokenResponseDto>.Ok(result));
    }
    catch (InvalidOperationException ex)
    {
        return TypedResults.BadRequest(ApiResponse<TokenResponseDto>.Fail(ex.Message));
    }
});

app.MapPost("/auth/login", async Task<Results<Ok<ApiResponse<TokenResponseDto>>, UnauthorizedHttpResult>> (
    LoginUserDto dto, HttpContext http, CancellationToken ct) =>
{
    var userService = http.RequestServices.GetRequiredService<IUserService>();
    try
    {
        var result = await userService.LoginAsync(dto, ct);
        return TypedResults.Ok(ApiResponse<TokenResponseDto>.Ok(result));
    }
    catch (UnauthorizedAccessException)
    {
        return TypedResults.Unauthorized();
    }
});

app.MapGet("/users", async (HttpContext http, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var userService = http.RequestServices.GetRequiredService<IUserService>();
    var result = await userService.GetAllAsync(page, pageSize, ct);
    return Results.Ok(ApiResponse<PagedResponse<UserResponseDto>>.Ok(result));
});

app.MapGet("/users/{id:guid}", async Task<Results<Ok<ApiResponse<UserResponseDto>>, NotFound>> (
    Guid id, HttpContext http, CancellationToken ct) =>
{
    var userService = http.RequestServices.GetRequiredService<IUserService>();
    try
    {
        var result = await userService.GetByIdAsync(id, ct);
        return TypedResults.Ok(ApiResponse<UserResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPut("/users/{id:guid}", async Task<Results<Ok<ApiResponse<UserResponseDto>>, NotFound>> (
    Guid id, UpdateUserDto dto, HttpContext http, CancellationToken ct) =>
{
    var userService = http.RequestServices.GetRequiredService<IUserService>();
    try
    {
        var result = await userService.UpdateAsync(id, dto, ct);
        return TypedResults.Ok(ApiResponse<UserResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPut("/users/{id:guid}/password", async Task<Results<Ok<string>, UnauthorizedHttpResult, NotFound>> (
    Guid id, ChangePasswordDto dto, HttpContext http, CancellationToken ct) =>
{
    var userService = http.RequestServices.GetRequiredService<IUserService>();
    try
    {
        await userService.ChangePasswordAsync(id, dto, ct);
        return TypedResults.Ok("Пароль успешно изменён");
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
    catch (UnauthorizedAccessException)
    {
        return TypedResults.Unauthorized();
    }
});

app.MapPost("/auth/refresh", async Task<Results<Ok<ApiResponse<RefreshResponseDto>>, UnauthorizedHttpResult, BadRequest<ApiResponse<RefreshResponseDto>>>> (
    RefreshRequestDto dto, HttpContext http, CancellationToken ct) =>
{
    var userService = http.RequestServices.GetRequiredService<IUserService>();
    try
    {
        var result = await userService.RefreshTokenAsync(dto, ct);
        return TypedResults.Ok(ApiResponse<RefreshResponseDto>.Ok(result));
    }
    catch (UnauthorizedAccessException)
    {
        return TypedResults.Unauthorized();
    }
    catch (Exception ex)
    {
        return TypedResults.BadRequest(ApiResponse<RefreshResponseDto>.Fail(ex.Message));
    }
});

app.MapPost("/auth/logout", async Task<Results<Ok<string>, NotFound, UnauthorizedHttpResult>> (
    HttpContext http, CancellationToken ct) =>
{
    var userService = http.RequestServices.GetRequiredService<IUserService>();
    var userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        return TypedResults.Unauthorized();

    try
    {
        await userService.LogoutAsync(userId, ct);
        return TypedResults.Ok("Успешный выход из системы");
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.Run();
