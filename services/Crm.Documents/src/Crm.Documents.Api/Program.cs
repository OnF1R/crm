using Crm.Documents.Application.DTOs;
using Crm.Documents.Application.Interfaces;
using Crm.Documents.Infrastructure;
using Crm.Documents.Infrastructure.Persistence;
using Crm.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDocumentsInfrastructure(builder.Configuration);
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
    var db = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseCors();
app.UseAuthentication();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Documents" }));

app.MapGet("/documents", async (HttpContext http, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<IDocumentService>();
    var result = await service.GetAllAsync(page, pageSize, ct);
    return Results.Ok(ApiResponse<PagedResponse<DocumentResponseDto>>.Ok(result));
});

app.MapGet("/documents/{id:guid}", async Task<Results<Ok<ApiResponse<DocumentResponseDto>>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDocumentService>();
    try { return TypedResults.Ok(ApiResponse<DocumentResponseDto>.Ok(await service.GetByIdAsync(id, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/documents/{id:guid}/download", async Task<Results<FileStreamHttpResult, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDocumentService>();
    try
    {
        var (stream, contentType, fileName) = await service.DownloadAsync(id, ct);
        return TypedResults.File(stream, contentType, fileName);
    }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/documents/by-entity", async (string type, Guid id, HttpContext http, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<IDocumentService>();
    var result = await service.GetByRelatedEntityAsync(type, id, page, pageSize, ct);
    return Results.Ok(ApiResponse<PagedResponse<DocumentResponseDto>>.Ok(result));
});

app.MapPost("/documents/upload", async Task<Results<Ok<ApiResponse<DocumentResponseDto>>, BadRequest<ApiResponse<DocumentResponseDto>>>> (
    IFormFile file, HttpContext http, string? relatedEntityType = null, Guid? relatedEntityId = null, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<IDocumentService>();
    try
    {
        if (string.IsNullOrEmpty(relatedEntityType) && http.Request.Query.ContainsKey("relatedEntityType"))
            relatedEntityType = http.Request.Query["relatedEntityType"];
        if (relatedEntityId == null && http.Request.Query.ContainsKey("relatedEntityId"))
            relatedEntityId = Guid.Parse(http.Request.Query["relatedEntityId"]!);

        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
        using var stream = file.OpenReadStream();
        var result = await service.UploadAsync(stream, file.FileName, file.ContentType, Guid.Parse(userId), relatedEntityType, relatedEntityId, ct);
        return TypedResults.Ok(ApiResponse<DocumentResponseDto>.Ok(result));
    }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<DocumentResponseDto>.Fail(ex.Message)); }
}).DisableAntiforgery();

app.MapDelete("/documents/{id:guid}", async Task<Results<Ok<string>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IDocumentService>();
    try { await service.DeleteAsync(id, ct); return TypedResults.Ok("Документ удален"); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.Run();
