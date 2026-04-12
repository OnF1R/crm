using Crm.Deals.Application.DTOs;
using Crm.Deals.Application.Interfaces;
using Crm.Deals.Infrastructure;
using Crm.Deals.Infrastructure.Persistence;
using Crm.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDealsInfrastructure(builder.Configuration);
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
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DealsDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseCors();
app.UseAuthentication();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Deals" }));

app.MapGet("/deals", async (HttpContext http, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    var result = await service.GetAllAsync(page, pageSize, ct);
    return Results.Ok(ApiResponse<PagedResponse<DealResponseDto>>.Ok(result));
});

app.MapGet("/deals/{id:guid}", async Task<Results<Ok<ApiResponse<DealResponseDto>>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try { return TypedResults.Ok(ApiResponse<DealResponseDto>.Ok(await service.GetByIdAsync(id, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/clients/{clientId:guid}/deals", async (Guid clientId, HttpContext http, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    var result = await service.GetByClientIdAsync(clientId, page, pageSize, ct);
    return Results.Ok(ApiResponse<PagedResponse<DealResponseDto>>.Ok(result));
});

app.MapPost("/deals", async Task<Results<Ok<ApiResponse<DealResponseDto>>, BadRequest<ApiResponse<DealResponseDto>>>> (
    CreateDealDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try
    {
        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
        var result = await service.CreateAsync(dto, Guid.Parse(userId), ct);
        return TypedResults.Ok(ApiResponse<DealResponseDto>.Ok(result));
    }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<DealResponseDto>.Fail(ex.Message)); }
});

app.MapPut("/deals/{id:guid}", async Task<Results<Ok<ApiResponse<DealResponseDto>>, NotFound>> (
    Guid id, UpdateDealDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try { return TypedResults.Ok(ApiResponse<DealResponseDto>.Ok(await service.UpdateAsync(id, dto, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapPut("/deals/{id:guid}/stage", async Task<Results<Ok<ApiResponse<DealResponseDto>>, NotFound>> (
    Guid id, MoveDealStageDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try
    {
        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
        return TypedResults.Ok(ApiResponse<DealResponseDto>.Ok(await service.MoveStageAsync(id, dto, Guid.Parse(userId), ct)));
    }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapDelete("/deals/{id:guid}", async Task<Results<Ok<string>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try { await service.DeleteAsync(id, ct); return TypedResults.Ok("Сделка удалена"); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.Run();
