using Crm.Identity.Application.DTOs;
using Crm.Identity.Domain.Entities;

namespace Crm.Identity.Application.Interfaces;

public interface ITokenService
{
    TokenResponseDto GenerateToken(User user);
}
