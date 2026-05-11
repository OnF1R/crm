using Crm.Clients.Application.DTOs;
using Crm.Clients.Application.Interfaces;
using Crm.Clients.Infrastructure;
using Crm.Clients.Infrastructure.Persistence;
using Crm.Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddClientsInfrastructure(builder.Configuration);
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
    var db = scope.ServiceProvider.GetRequiredService<ClientsDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors();
app.UseAuthentication();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Clients" }));

app.MapGet("/clients", async (HttpContext http, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    var result = await service.GetAllAsync(page, pageSize, ct);
    return Results.Ok(ApiResponse<PagedResponse<ClientResponseDto>>.Ok(result));
});

app.MapGet("/clients/{id:guid}", async Task<Results<Ok<ApiResponse<ClientResponseDto>>, NotFound>> (
    Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        var result = await service.GetByIdAsync(id, ct);
        return TypedResults.Ok(ApiResponse<ClientResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPost("/clients", async Task<Results<Ok<ApiResponse<ClientResponseDto>>, BadRequest<ApiResponse<ClientResponseDto>>>> (
    CreateClientDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
        var result = await service.CreateAsync(dto, Guid.Parse(userId), ct);
        return TypedResults.Ok(ApiResponse<ClientResponseDto>.Ok(result));
    }
    catch (Exception ex)
    {
        return TypedResults.BadRequest(ApiResponse<ClientResponseDto>.Fail(ex.Message));
    }
});

app.MapPut("/clients/{id:guid}", async Task<Results<Ok<ApiResponse<ClientResponseDto>>, NotFound>> (
    Guid id, UpdateClientDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        var result = await service.UpdateAsync(id, dto, ct);
        return TypedResults.Ok(ApiResponse<ClientResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPut("/clients/{id:guid}/status", async Task<Results<Ok<ApiResponse<ClientResponseDto>>, BadRequest<ApiResponse<ClientResponseDto>>, NotFound>>(
    Guid id, UpdateClientStatusDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        var result = await service.SetStatusAsync(id, dto.Status, ct);
        return TypedResults.Ok(ApiResponse<ClientResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return TypedResults.BadRequest(ApiResponse<ClientResponseDto>.Fail(ex.Message));
    }
});

app.MapDelete("/clients/{id:guid}", async Task<Results<Ok<string>, NotFound>> (
    Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        await service.DeleteAsync(id, ct);
        return TypedResults.Ok("Клиент удалён");
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPost("/clients/{clientId:guid}/contacts", async Task<Results<Ok<ApiResponse<ContactResponseDto>>, NotFound>> (
    Guid clientId, CreateContactDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        var result = await service.AddContactAsync(clientId, dto, ct);
        return TypedResults.Ok(ApiResponse<ContactResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPut("/contacts/{contactId:guid}", async Task<Results<Ok<ApiResponse<ContactResponseDto>>, NotFound>> (
    Guid contactId, UpdateContactDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        var result = await service.UpdateContactAsync(contactId, dto, ct);
        return TypedResults.Ok(ApiResponse<ContactResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapDelete("/contacts/{contactId:guid}", async Task<Results<Ok<string>, NotFound>> (
    Guid contactId, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        await service.DeleteContactAsync(contactId, ct);
        return TypedResults.Ok("Контакт удалён");
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

// Tag endpoints
app.MapGet("/tags", async (HttpContext http, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    var result = await service.GetAllTagsAsync(ct);
    return Results.Ok(ApiResponse<IReadOnlyList<TagResponseDto>>.Ok(result));
});

app.MapGet("/tags/{id:guid}", async Task<Results<Ok<ApiResponse<TagResponseDto>>, NotFound>> (
    Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        var result = await service.GetTagByIdAsync(id, ct);
        return TypedResults.Ok(ApiResponse<TagResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPost("/tags", async Task<Results<Ok<ApiResponse<TagResponseDto>>, BadRequest<ApiResponse<TagResponseDto>>>> (
    CreateTagDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        var result = await service.CreateTagAsync(dto, ct);
        return TypedResults.Ok(ApiResponse<TagResponseDto>.Ok(result));
    }
    catch (Exception ex)
    {
        return TypedResults.BadRequest(ApiResponse<TagResponseDto>.Fail(ex.Message));
    }
});

app.MapPut("/tags/{id:guid}", async Task<Results<Ok<ApiResponse<TagResponseDto>>, NotFound, BadRequest<ApiResponse<TagResponseDto>>>> (
    Guid id, UpdateTagDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        var result = await service.UpdateTagAsync(id, dto, ct);
        return TypedResults.Ok(ApiResponse<TagResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
    catch (Exception ex)
    {
        return TypedResults.BadRequest(ApiResponse<TagResponseDto>.Fail(ex.Message));
    }
});

app.MapDelete("/tags/{id:guid}", async Task<Results<Ok<string>, NotFound>> (
    Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        await service.DeleteTagAsync(id, ct);
        return TypedResults.Ok("Тег удалён");
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapPost("/clients/{clientId:guid}/tags/{tagId:guid}", async Task<Results<Ok<string>, NotFound>> (
    Guid clientId, Guid tagId, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        await service.AddTagToClientAsync(clientId, tagId, ct);
        return TypedResults.Ok("Тег добавлен к клиенту");
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.MapDelete("/clients/{clientId:guid}/tags/{tagId:guid}", async Task<Results<Ok<string>, NotFound>> (
    Guid clientId, Guid tagId, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<IClientService>();
    try
    {
        await service.RemoveTagFromClientAsync(clientId, tagId, ct);
        return TypedResults.Ok("Тег удалён из клиента");
    }
    catch (KeyNotFoundException)
    {
        return TypedResults.NotFound();
    }
});

app.Run();
