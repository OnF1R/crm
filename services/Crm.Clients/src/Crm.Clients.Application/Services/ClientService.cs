using Crm.Clients.Application.DTOs;
using Crm.Clients.Application.Interfaces;
using Crm.Clients.Domain.Entities;
using Crm.Clients.Domain.Interfaces;
using Crm.Shared.Domain;
using Crm.Shared.DTOs;

namespace Crm.Clients.Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(IClientRepository clientRepository, IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ClientResponseDto> CreateAsync(CreateClientDto dto, Guid createdBy, CancellationToken ct = default)
    {
        var client = Client.Create(dto.CompanyName, dto.Inn, dto.Website, dto.Industry, dto.AssignedUserId, createdBy);
        await _clientRepository.AddAsync(client, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(client);
    }

    public async Task<ClientResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetWithContactsAsync(id, ct)
            ?? throw new KeyNotFoundException("Клиент не найден");
        return MapToResponse(client);
    }

    public async Task<PagedResponse<ClientResponseDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var clients = await _clientRepository.GetAllAsync(ct);
        var total = clients.Count;
        var paged = clients
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToResponse)
            .ToList();
        return new PagedResponse<ClientResponseDto>(paged, total, page, pageSize);
    }

    public async Task<ClientResponseDto> UpdateAsync(Guid id, UpdateClientDto dto, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Клиент не найден");
        client.Update(dto.CompanyName, dto.Inn, dto.Website, dto.Industry, dto.AssignedUserId);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapToResponse(client);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Клиент не найден");
        await _clientRepository.DeleteAsync(client, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<ContactResponseDto> AddContactAsync(Guid clientId, CreateContactDto dto, CancellationToken ct = default)
    {
        var client = await _clientRepository.GetByIdAsync(clientId, ct)
            ?? throw new KeyNotFoundException("Клиент не найден");
        var contact = client.AddContact(dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.Position, dto.IsPrimary);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapContactToResponse(contact);
    }

    public async Task<ContactResponseDto> UpdateContactAsync(Guid contactId, UpdateContactDto dto, CancellationToken ct = default)
    {
        var clients = await _clientRepository.GetAllAsync(ct);
        var client = clients.FirstOrDefault(c => c.Contacts.Any(co => co.Id == contactId))
            ?? throw new KeyNotFoundException("Контакт не найден");
        var contact = client.Contacts.First(co => co.Id == contactId);
        contact.Update(dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.Position, dto.IsPrimary);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapContactToResponse(contact);
    }

    public async Task DeleteContactAsync(Guid contactId, CancellationToken ct = default)
    {
        var clients = await _clientRepository.GetAllAsync(ct);
        var client = clients.FirstOrDefault(c => c.Contacts.Any(co => co.Id == contactId))
            ?? throw new KeyNotFoundException("Контакт не найден");
        var contact = client.Contacts.First(co => co.Id == contactId);
        client.Contacts.ToList().Remove(contact);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    private static ClientResponseDto MapToResponse(Client client) => new(
        client.Id,
        client.CompanyName,
        client.Inn,
        client.Website,
        client.Industry.ToString(),
        client.Status.ToString(),
        client.AssignedUserId,
        client.CreatedAt,
        client.Contacts.Select(MapContactToResponse).ToList());

    private static ContactResponseDto MapContactToResponse(Contact contact) => new(
        contact.Id,
        contact.ClientId,
        contact.FirstName,
        contact.LastName,
        contact.GetFullName(),
        contact.Email,
        contact.Phone,
        contact.Position,
        contact.IsPrimary);
}
