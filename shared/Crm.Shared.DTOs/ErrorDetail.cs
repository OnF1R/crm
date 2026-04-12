namespace Crm.Shared.DTOs;

public record ErrorDetail(string Code, string Message, string? Target = null);
