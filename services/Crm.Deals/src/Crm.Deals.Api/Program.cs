using Crm.Deals.Application.DTOs;
using Crm.Deals.Application.Interfaces;
using Crm.Deals.Infrastructure;
using Crm.Deals.Infrastructure.Persistence;
using Crm.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
    await db.Database.MigrateAsync();
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

// Refusal reason endpoints
app.MapGet("/refusal-reasons", async (HttpContext http, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    var result = await service.GetAllRefusalReasonsAsync(ct);
    return Results.Ok(ApiResponse<IReadOnlyList<RefusalReasonResponseDto>>.Ok(result));
});

app.MapGet("/refusal-reasons/{id:guid}", async Task<Results<Ok<ApiResponse<RefusalReasonResponseDto>>, NotFound>> (
    Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try
    {
        var result = await service.GetRefusalReasonByIdAsync(id, ct);
        return TypedResults.Ok(ApiResponse<RefusalReasonResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPost("/refusal-reasons", async Task<Results<Ok<ApiResponse<RefusalReasonResponseDto>>, BadRequest<ApiResponse<RefusalReasonResponseDto>>>> (
    CreateRefusalReasonDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try
    {
        var result = await service.CreateRefusalReasonAsync(dto, ct);
        return TypedResults.Ok(ApiResponse<RefusalReasonResponseDto>.Ok(result));
    }
    catch (Exception ex)
    {
        return TypedResults.BadRequest(ApiResponse<RefusalReasonResponseDto>.Fail(ex.Message));
    }
});

app.MapPut("/refusal-reasons/{id:guid}", async Task<Results<Ok<ApiResponse<RefusalReasonResponseDto>>, NotFound, BadRequest<ApiResponse<RefusalReasonResponseDto>>>> (
    Guid id, UpdateRefusalReasonDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try
    {
        var result = await service.UpdateRefusalReasonAsync(id, dto, ct);
        return TypedResults.Ok(ApiResponse<RefusalReasonResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
    catch (Exception ex)
    {
        return TypedResults.BadRequest(ApiResponse<RefusalReasonResponseDto>.Fail(ex.Message));
    }
});

app.MapDelete("/refusal-reasons/{id:guid}", async Task<Results<Ok<string>, NotFound>> (
    Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try
    {
        await service.DeleteRefusalReasonAsync(id, ct);
        return TypedResults.Ok("Причина отказа удалена");
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPut("/deals/{dealId:guid}/refusal-reason", async Task<Results<Ok<ApiResponse<string>>, NotFound>> (
    Guid dealId, SetDealRefusalReasonDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDealService>();
    try
    {
        await service.SetDealRefusalReasonAsync(dealId, dto.RefusalReasonId, ct);
        return TypedResults.Ok(ApiResponse<string>.Ok("Причина отказа установлена"));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.Run();
