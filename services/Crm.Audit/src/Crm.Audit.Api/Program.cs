using Crm.Shared.DTOs;
using Crm.Audit.Application.DTOs;
using Crm.Audit.Domain.Interfaces;
using Crm.Audit.Infrastructure;
using Crm.Audit.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuditInfrastructure(builder.Configuration);
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
    var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseCors();
app.UseAuthentication();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Audit" }));

app.MapPost("/audit/log", async Task<Results<Ok<string>, BadRequest<ApiResponse<AuditLogResponseDto>>>> (
    CreateAuditLogDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IAuditService>();
    try
    {
        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
        await service.LogAsync(dto.ServiceName, dto.EntityName, dto.EntityId, dto.Action, Guid.Parse(userId), dto.UserName, dto.Changes, dto.IpAddress, ct);
        return TypedResults.Ok("Audit log created");
    }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<AuditLogResponseDto>.Fail(ex.Message)); }
});

app.MapGet("/audit/logs/service/{serviceName}", async Task<Results<Ok<ApiResponse<IReadOnlyList<AuditLogResponseDto>>>, NotFound>> (string serviceName, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IAuditLogRepository>();
    try
    {
        var logs = await service.GetByServiceAsync(serviceName, ct);
        var response = logs.Select(l => new AuditLogResponseDto(l.Id, l.ServiceName, l.EntityName, l.EntityId, l.Action, l.UserId, l.UserName, l.Changes, l.IpAddress, l.CreatedAt)).ToList();
        return TypedResults.Ok(ApiResponse<IReadOnlyList<AuditLogResponseDto>>.Ok(response));
    }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/audit/logs/user/{userId:guid}", async Task<Results<Ok<ApiResponse<IReadOnlyList<AuditLogResponseDto>>>, NotFound>> (Guid userId, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IAuditLogRepository>();
    try
    {
        var logs = await service.GetByUserAsync(userId, ct);
        var response = logs.Select(l => new AuditLogResponseDto(l.Id, l.ServiceName, l.EntityName, l.EntityId, l.Action, l.UserId, l.UserName, l.Changes, l.IpAddress, l.CreatedAt)).ToList();
        return TypedResults.Ok(ApiResponse<IReadOnlyList<AuditLogResponseDto>>.Ok(response));
    }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/audit/logs/entity/{entityName}/{entityId:guid}", async Task<Results<Ok<ApiResponse<IReadOnlyList<AuditLogResponseDto>>>, NotFound>> (string entityName, Guid entityId, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IAuditLogRepository>();
    try
    {
        var logs = await service.GetByEntityAsync(entityName, entityId, ct);
        var response = logs.Select(l => new AuditLogResponseDto(l.Id, l.ServiceName, l.EntityName, l.EntityId, l.Action, l.UserId, l.UserName, l.Changes, l.IpAddress, l.CreatedAt)).ToList();
        return TypedResults.Ok(ApiResponse<IReadOnlyList<AuditLogResponseDto>>.Ok(response));
    }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/audit/logs/range", async Task<Results<Ok<ApiResponse<IReadOnlyList<AuditLogResponseDto>>>, BadRequest<ApiResponse<IReadOnlyList<AuditLogResponseDto>>>>> (
    DateTime from, DateTime to, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IAuditLogRepository>();
    try
    {
        var logs = await service.GetByDateRangeAsync(from, to, ct);
        var response = logs.Select(l => new AuditLogResponseDto(l.Id, l.ServiceName, l.EntityName, l.EntityId, l.Action, l.UserId, l.UserName, l.Changes, l.IpAddress, l.CreatedAt)).ToList();
        return TypedResults.Ok(ApiResponse<IReadOnlyList<AuditLogResponseDto>>.Ok(response));
    }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<IReadOnlyList<AuditLogResponseDto>>.Fail(ex.Message)); }
});

app.Run();
