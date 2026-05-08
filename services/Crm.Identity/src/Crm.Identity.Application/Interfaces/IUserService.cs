using Crm.Identity.Application.DTOs;
using Crm.Shared.DTOs;

namespace Crm.Identity.Application.Interfaces;

public interface IUserService
{
    Task<TokenResponseDto> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default);
    Task<TokenResponseDto> LoginAsync(LoginUserDto dto, CancellationToken ct = default);
    Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResponse<UserResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid id, ChangePasswordDto dto, CancellationToken ct = default);
    Task<RefreshResponseDto> RefreshTokenAsync(RefreshRequestDto dto, CancellationToken ct = default);
    Task LogoutAsync(Guid userId, CancellationToken ct = default);
}
