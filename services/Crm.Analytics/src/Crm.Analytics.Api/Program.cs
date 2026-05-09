using Crm.Analytics.Application.DTOs;
using Crm.Analytics.Application.Interfaces;
using Crm.Analytics.Domain.Entities;
using Crm.Analytics.Infrastructure;
using Crm.Analytics.Infrastructure.Persistence;
using Crm.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAnalyticsInfrastructure(builder.Configuration);
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
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors();
app.UseAuthentication();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Analytics" }));

app.MapGet("/dashboard/{userId:guid}", async (Guid userId, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IAnalyticsService>();
    var result = await service.GetDashboardAsync(userId, ct);
    return Results.Ok(ApiResponse<DashboardDto>.Ok(result));
});

app.MapGet("/reports", async (HttpContext http, CancellationToken ct, int take = 20) =>
{
    var service = http.RequestServices.GetRequiredService<IAnalyticsService>();
    var result = await service.GetRecentReportsAsync(take, ct);
    return Results.Ok(ApiResponse<IReadOnlyList<ReportResponseDto>>.Ok(result));
});

app.MapPost("/reports", async Task<Results<Ok<ApiResponse<ReportResponseDto>>, BadRequest<ApiResponse<ReportResponseDto>>>> (
    ReportRequestDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IAnalyticsService>();
    try
    {
        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
        var result = await service.GenerateReportAsync(
            dto.Type,
            dto.Parameters,
            Guid.Parse(userId),
            dto.Data,
            ct);
        return TypedResults.Ok(ApiResponse<ReportResponseDto>.Ok(result));
    }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<ReportResponseDto>.Fail(ex.Message)); }
});

app.Run();
