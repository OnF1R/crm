using Crm.Notifications.Application.DTOs;
using Crm.Notifications.Application.Interfaces;
using Crm.Notifications.Infrastructure;
using Crm.Notifications.Infrastructure.Persistence;
using Crm.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNotificationsInfrastructure(builder.Configuration);
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
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseCors();
app.UseAuthentication();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Notifications" }));

app.MapGet("/notifications/{id:guid}", async Task<Results<Ok<ApiResponse<NotificationResponseDto>>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<INotificationService>();
    try { return TypedResults.Ok(ApiResponse<NotificationResponseDto>.Ok(await service.GetByIdAsync(id, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/users/{userId:guid}/notifications", async (Guid userId, HttpContext http, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<INotificationService>();
    var result = await service.GetByUserIdAsync(userId, page, pageSize, ct);
    return Results.Ok(ApiResponse<PagedResponse<NotificationResponseDto>>.Ok(result));
});

app.MapGet("/users/{userId:guid}/notifications/unread-count", async (Guid userId, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<INotificationService>();
    var result = await service.GetUnreadCountAsync(userId, ct);
    return Results.Ok(ApiResponse<UnreadCountDto>.Ok(result));
});

app.MapPost("/notifications", async Task<Results<Ok<ApiResponse<NotificationResponseDto>>, BadRequest<ApiResponse<NotificationResponseDto>>>> (
    CreateNotificationDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<INotificationService>();
    try
    {
        var result = await service.CreateAsync(dto, ct);
        return TypedResults.Ok(ApiResponse<NotificationResponseDto>.Ok(result));
    }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<NotificationResponseDto>.Fail(ex.Message)); }
});

app.MapPut("/notifications/{id:guid}/read", async Task<Results<Ok<ApiResponse<NotificationResponseDto>>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<INotificationService>();
    try { return TypedResults.Ok(ApiResponse<NotificationResponseDto>.Ok(await service.MarkAsReadAsync(id, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapDelete("/notifications/{id:guid}", async Task<Results<Ok<string>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<INotificationService>();
    try { await service.DeleteAsync(id, ct); return TypedResults.Ok("Уведомление удалено"); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.Run();
