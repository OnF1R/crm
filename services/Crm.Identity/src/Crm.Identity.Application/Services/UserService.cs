using Crm.Identity.Application.DTOs;
using Crm.Identity.Application.Interfaces;
using Crm.Identity.Domain.Entities;
using Crm.Identity.Domain.Enums;
using Crm.Identity.Domain.Interfaces;
using Crm.Shared.Domain;
using Crm.Shared.DTOs;

namespace Crm.Identity.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterUserDto dto, CancellationToken ct = default)
    {
        if (await _userRepository.ExistsByEmailAsync(dto.Email, ct))
            throw new InvalidOperationException("Пользователь с таким email уже существует");

        var passwordHash = _passwordHasher.HashPassword(dto.Password);
        var role = (UserRole)dto.Role;
        var user = User.Create(dto.Email, passwordHash, dto.FirstName, dto.LastName, role, Guid.Empty);

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var refreshToken = GenerateRefreshToken(user);
        await _refreshTokenRepository.AddAsync(refreshToken, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _tokenService.GenerateToken(user, refreshToken.Token);
    }

    public async Task<TokenResponseDto> LoginAsync(LoginUserDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email, ct)
            ?? throw new UnauthorizedAccessException("Неверный email или пароль");

        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный email или пароль");

        var refreshToken = GenerateRefreshToken(user);
        await _refreshTokenRepository.AddAsync(refreshToken, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _tokenService.GenerateToken(user, refreshToken.Token);
    }

    public async Task<UserResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Пользователь не найден");

        return MapToResponse(user);
    }

    public async Task<PagedResponse<UserResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var users = await _userRepository.GetAllAsync(ct);
        var total = users.Count;
        var paged = users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToResponse)
            .ToList();

        return new PagedResponse<UserResponseDto>(paged, total, page, pageSize);
    }

    public async Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Пользователь не найден");

        user.Update(dto.FirstName, dto.LastName, (UserRole)dto.Role);
        await _unitOfWork.SaveChangesAsync(ct);

        return MapToResponse(user);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordDto dto, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Пользователь не найден");

        if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный текущий пароль");

        user.ChangePassword(_passwordHasher.HashPassword(dto.NewPassword));
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<RefreshResponseDto> RefreshTokenAsync(RefreshRequestDto dto, CancellationToken ct = default)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken, ct)
            ?? throw new UnauthorizedAccessException("Неверный refresh token");

        if (!refreshToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token неактивен или истек");

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, ct)
            ?? throw new KeyNotFoundException("Пользователь не найден");

        // Revoke old refresh token
        refreshToken.Revoke("Заменен новым токеном");
        await _refreshTokenRepository.UpdateAsync(refreshToken, ct);

        // Generate new refresh token
        var newRefreshToken = GenerateRefreshToken(user);
        await _refreshTokenRepository.AddAsync(newRefreshToken, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return new RefreshResponseDto(
            _tokenService.GenerateAccessToken(user),
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        if (await _userRepository.GetByIdAsync(userId, ct) == null)
            throw new KeyNotFoundException("Пользователь не найден");

        var activeTokens = await _refreshTokenRepository.GetActiveTokensAsync(userId, ct);
        foreach (var token in activeTokens)
        {
            token.Revoke("Пользователь вышел из системы");
            await _refreshTokenRepository.UpdateAsync(token, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static RefreshToken GenerateRefreshToken(User user)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var expiresAt = DateTime.UtcNow.AddDays(7);
        return RefreshToken.Create(user.Id, token, expiresAt);
    }

    private static UserResponseDto MapToResponse(User user) => new(
        user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.GetFullName(),
        (int)user.Role,
        user.Role.ToString(),
        user.Status.ToString(),
        user.CreatedAt);
}
