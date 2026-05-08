using Crm.Shared.DTOs;
using Crm.Tasks.Application.DTOs;
using Crm.Tasks.Application.Interfaces;
using Crm.Tasks.Infrastructure;
using Crm.Tasks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTasksInfrastructure(builder.Configuration);
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
    var db = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors();
app.UseAuthentication();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "Tasks" }));

app.MapGet("/tasks", async (HttpContext http, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    var result = await service.GetAllAsync(page, pageSize, ct);
    return Results.Ok(ApiResponse<PagedResponse<TaskResponseDto>>.Ok(result));
});

app.MapGet("/tasks/{id:guid}", async Task<Results<Ok<ApiResponse<TaskResponseDto>>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { return TypedResults.Ok(ApiResponse<TaskResponseDto>.Ok(await service.GetByIdAsync(id, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/users/{userId:guid}/tasks", async (Guid userId, HttpContext http, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    var result = await service.GetByAssignedUserIdAsync(userId, page, pageSize, ct);
    return Results.Ok(ApiResponse<PagedResponse<TaskResponseDto>>.Ok(result));
});

app.MapPost("/tasks", async Task<Results<Ok<ApiResponse<TaskResponseDto>>, BadRequest<ApiResponse<TaskResponseDto>>>> (
    CreateTaskDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try
    {
        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
        var result = await service.CreateAsync(dto, Guid.Parse(userId), ct);
        return TypedResults.Ok(ApiResponse<TaskResponseDto>.Ok(result));
    }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<TaskResponseDto>.Fail(ex.Message)); }
});

app.MapPut("/tasks/{id:guid}", async Task<Results<Ok<ApiResponse<TaskResponseDto>>, NotFound>> (
    Guid id, UpdateTaskDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { return TypedResults.Ok(ApiResponse<TaskResponseDto>.Ok(await service.UpdateAsync(id, dto, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapPut("/tasks/{id:guid}/status", async Task<Results<Ok<ApiResponse<TaskResponseDto>>, NotFound>> (
    Guid id, SetTaskStatusDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { return TypedResults.Ok(ApiResponse<TaskResponseDto>.Ok(await service.SetStatusAsync(id, dto, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapDelete("/tasks/{id:guid}", async Task<Results<Ok<string>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { await service.DeleteAsync(id, ct); return TypedResults.Ok("Задача удалена"); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/tasks/{id:guid}/comments", async Task<Results<Ok<ApiResponse<IReadOnlyList<CommentResponseDto>>>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { return TypedResults.Ok(ApiResponse<IReadOnlyList<CommentResponseDto>>.Ok(await service.GetCommentsAsync(id, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapPost("/tasks/{id:guid}/comments", async Task<Results<Ok<ApiResponse<CommentResponseDto>>, BadRequest<ApiResponse<CommentResponseDto>>>> (
    Guid id, CreateCommentDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try
    {
        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
        var result = await service.AddCommentAsync(id, dto, Guid.Parse(userId), ct);
        return TypedResults.Ok(ApiResponse<CommentResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException) { return TypedResults.BadRequest(ApiResponse<CommentResponseDto>.Fail("Задача не найдена")); }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<CommentResponseDto>.Fail(ex.Message)); }
});

// SubTask endpoints
app.MapGet("/tasks/{id:guid}/subtasks", async Task<Results<Ok<ApiResponse<IReadOnlyList<SubTaskResponseDto>>>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { return TypedResults.Ok(ApiResponse<IReadOnlyList<SubTaskResponseDto>>.Ok(await service.GetSubTasksAsync(id, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapGet("/tasks/{id:guid}/with-subtasks", async Task<Results<Ok<ApiResponse<TaskResponseDto>>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { return TypedResults.Ok(ApiResponse<TaskResponseDto>.Ok(await service.GetByIdWithSubTasksAsync(id, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapPost("/tasks/{id:guid}/subtasks", async Task<Results<Ok<ApiResponse<SubTaskResponseDto>>, BadRequest<ApiResponse<SubTaskResponseDto>>>> (
    Guid id, CreateSubTaskDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try
    {
        var userId = http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString();
        var result = await service.AddSubTaskAsync(id, dto, Guid.Parse(userId), ct);
        return TypedResults.Ok(ApiResponse<SubTaskResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException) { return TypedResults.BadRequest(ApiResponse<SubTaskResponseDto>.Fail("Родительская задача не найдена")); }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<SubTaskResponseDto>.Fail(ex.Message)); }
});

app.MapPut("/subtasks/{id:guid}", async Task<Results<Ok<ApiResponse<SubTaskResponseDto>>, NotFound>> (
    Guid id, UpdateSubTaskDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { return TypedResults.Ok(ApiResponse<SubTaskResponseDto>.Ok(await service.UpdateSubTaskAsync(id, dto, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapPut("/subtasks/{id:guid}/status", async Task<Results<Ok<ApiResponse<SubTaskResponseDto>>, NotFound>> (
    Guid id, SetTaskStatusDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { return TypedResults.Ok(ApiResponse<SubTaskResponseDto>.Ok(await service.SetSubTaskStatusAsync(id, dto, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapDelete("/subtasks/{id:guid}", async Task<Results<Ok<string>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { await service.DeleteSubTaskAsync(id, ct); return TypedResults.Ok("Подзадача удалена"); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapPut("/tasks/{id:guid}/subtasks/order", async Task<Results<Ok<string>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { await service.UpdateSubTasksOrderAsync(id, ct); return TypedResults.Ok("Порядок подзадач обновлен"); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

// TaskDependency endpoints
app.MapGet("/tasks/{id:guid}/dependencies", async Task<Results<Ok<ApiResponse<IReadOnlyList<TaskDependencyResponseDto>>>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { return TypedResults.Ok(ApiResponse<IReadOnlyList<TaskDependencyResponseDto>>.Ok(await service.GetDependenciesAsync(id, ct))); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.MapPost("/tasks/{id:guid}/dependencies", async Task<Results<Ok<ApiResponse<TaskDependencyResponseDto>>, BadRequest<ApiResponse<TaskDependencyResponseDto>>>> (
    Guid id, CreateTaskDependencyDto dto, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try
    {
        var result = await service.AddDependencyAsync(id, dto, ct);
        return TypedResults.Ok(ApiResponse<TaskDependencyResponseDto>.Ok(result));
    }
    catch (KeyNotFoundException) { return TypedResults.BadRequest(ApiResponse<TaskDependencyResponseDto>.Fail("Задача не найдена")); }
    catch (ArgumentException ex) { return TypedResults.BadRequest(ApiResponse<TaskDependencyResponseDto>.Fail(ex.Message)); }
    catch (Exception ex) { return TypedResults.BadRequest(ApiResponse<TaskDependencyResponseDto>.Fail(ex.Message)); }
});

app.MapDelete("/dependencies/{id:guid}", async Task<Results<Ok<string>, NotFound>> (Guid id, HttpContext http, CancellationToken ct) =>
{
    var service = http.RequestServices.GetRequiredService<ITaskService>();
    try { await service.DeleteDependencyAsync(id, ct); return TypedResults.Ok("Зависимость удалена"); }
    catch (KeyNotFoundException) { return TypedResults.NotFound(); }
});

app.Run();
