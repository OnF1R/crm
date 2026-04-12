namespace Crm.Shared.DTOs;

public record ApiResponse<T>(T? Data, bool Success, string? Message = null, IReadOnlyList<ErrorDetail>? Errors = null)
{
    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new(data, true, message);

    public static ApiResponse<T> Fail(string message, IReadOnlyList<ErrorDetail>? errors = null) =>
        new(default, false, message, errors);

    public static ApiResponse<T> Fail(ErrorDetail error) =>
        new(default, false, error.Message, [error]);
}
