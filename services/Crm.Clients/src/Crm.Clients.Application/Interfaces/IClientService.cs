using Crm.Clients.Application.DTOs;
using Crm.Shared.DTOs;

namespace Crm.Clients.Application.Interfaces;

public interface IClientService
{
    Task<ClientResponseDto> CreateAsync(CreateClientDto dto, Guid createdBy, CancellationToken ct = default);
    Task<ClientResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResponse<ClientResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ClientResponseDto> UpdateAsync(Guid id, UpdateClientDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<ContactResponseDto> AddContactAsync(Guid clientId, CreateContactDto dto, CancellationToken ct = default);
    Task<ContactResponseDto> UpdateContactAsync(Guid contactId, UpdateContactDto dto, CancellationToken ct = default);
    Task DeleteContactAsync(Guid contactId, CancellationToken ct = default);

    // Tag methods
    Task<TagResponseDto> CreateTagAsync(CreateTagDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<TagResponseDto>> GetAllTagsAsync(CancellationToken ct = default);
    Task<TagResponseDto> GetTagByIdAsync(Guid id, CancellationToken ct = default);
    Task<TagResponseDto> UpdateTagAsync(Guid id, UpdateTagDto dto, CancellationToken ct = default);
    Task DeleteTagAsync(Guid id, CancellationToken ct = default);
    Task AddTagToClientAsync(Guid clientId, Guid tagId, CancellationToken ct = default);
    Task RemoveTagFromClientAsync(Guid clientId, Guid tagId, CancellationToken ct = default);
}
