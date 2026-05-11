using Crm.Clients.Application.DTOs;
using Crm.Clients.Application.Interfaces;
using Crm.Clients.Application.Services;
using Crm.Clients.Domain.Entities;
using Crm.Clients.Domain.Enums;
using Crm.Clients.Domain.Interfaces;
using Crm.Shared.Domain;
using Xunit;

namespace Crm.Clients.Tests;

public sealed class ClientServiceTests
{
    private static readonly Guid Creator = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid User = new("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnDto()
    {
        var clientRepo = new InMemoryClientRepository();
        var tagRepo = new InMemoryTagRepository();
        var uow = new FakeUnitOfWork();
        var service = new ClientService(clientRepo, tagRepo, uow);

        var request = new CreateClientDto("Acme", "Desc", "https://avatar", "123", "https://acme.com", (int)(Industry)1, User);
        var response = await service.CreateAsync(request, Creator, default);

        Assert.Equal("Acme", response.CompanyName);
        Assert.Equal((Industry)1, Enum.Parse<Industry>(response.Industry));
        Assert.NotEqual(string.Empty, response.Status);
        Assert.Equal(1, uow.SaveCount);
        Assert.Single(await clientRepo.GetAllAsync());
    }

    [Fact]
    public async Task AddContactAsync_ShouldCreateContactAndAddToClient()
    {
        var clientRepo = new InMemoryClientRepository();
        var tagRepo = new InMemoryTagRepository();
        var uow = new FakeUnitOfWork();
        var service = new ClientService(clientRepo, tagRepo, uow);
        var client = Client.Create("Acme", null, null, null, null, (Industry)1, User, Creator);
        await clientRepo.AddAsync(client);

        var contact = await service.AddContactAsync(client.Id, new CreateContactDto("Ivan", "Ivanov", "ivan@x", null, "PM", true));

        Assert.Equal(client.Id, contact.ClientId);
        Assert.Equal("Ivan Ivanov", contact.FullName);
        Assert.Single(client.Contacts);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task UpdateContactAsync_ShouldUpdateExistingContact()
    {
        var clientRepo = new InMemoryClientRepository();
        var tagRepo = new InMemoryTagRepository();
        var uow = new FakeUnitOfWork();
        var service = new ClientService(clientRepo, tagRepo, uow);
        var client = Client.Create("Acme", null, null, null, null, (Industry)1, User, Creator);
        var contact = client.AddContact("Ivan", "Ivanov", "old@x", null, "PM", false);
        await clientRepo.AddAsync(client);
        var contactId = contact.Id;

        var updated = await service.UpdateContactAsync(contactId, new UpdateContactDto("Petr", "Petrov", "new@x", "+7", "Lead", true));

        Assert.Equal(contact.Id, updated.Id);
        Assert.Equal("Petr", updated.FirstName);
        Assert.Equal("new@x", updated.Email);
        Assert.True(updated.IsPrimary);
    }

    [Fact]
    public async Task CreateTagAsync_ShouldThrowIfNameExists()
    {
        var clientRepo = new InMemoryClientRepository();
        var tagRepo = new InMemoryTagRepository();
        var existingTag = Tag.Create("VIP");
        await tagRepo.AddAsync(existingTag);
        var service = new ClientService(clientRepo, tagRepo, new FakeUnitOfWork());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateTagAsync(new CreateTagDto("VIP")));
    }

    [Fact]
    public async Task AddTagToClientAsync_ShouldAttachTag()
    {
        var clientRepo = new InMemoryClientRepository();
        var tagRepo = new InMemoryTagRepository();
        var uow = new FakeUnitOfWork();
        var service = new ClientService(clientRepo, tagRepo, uow);
        var client = Client.Create("Acme", null, null, null, null, (Industry)1, User, Creator);
        var tag = Tag.Create("VIP");
        await clientRepo.AddAsync(client);
        await tagRepo.AddAsync(tag);

        await service.AddTagToClientAsync(client.Id, tag.Id);

        Assert.Single(client.Tags);
        Assert.Equal(tag.Id, client.Tags[0].Id);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task RemoveTagFromClientAsync_ShouldDetachTag()
    {
        var clientRepo = new InMemoryClientRepository();
        var tagRepo = new InMemoryTagRepository();
        var uow = new FakeUnitOfWork();
        var service = new ClientService(clientRepo, tagRepo, uow);
        var client = Client.Create("Acme", null, null, null, null, (Industry)1, User, Creator);
        var tag = Tag.Create("VIP");
        client.AddTag(tag);
        await clientRepo.AddAsync(client);

        await service.RemoveTagFromClientAsync(client.Id, tag.Id);

        Assert.Empty(client.Tags);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task SetStatusAsync_ShouldSetStatusAndSaveChanges()
    {
        var clientRepo = new InMemoryClientRepository();
        var tagRepo = new InMemoryTagRepository();
        var uow = new FakeUnitOfWork();
        var service = new ClientService(clientRepo, tagRepo, uow);
        var client = Client.Create("Acme", null, null, null, null, (Industry)1, User, Creator);
        await clientRepo.AddAsync(client);

        var response = await service.SetStatusAsync(client.Id, 4);

        Assert.Equal("Закрытый", response.Status);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task SetStatusAsync_ShouldThrowForInvalidStatus()
    {
        var clientRepo = new InMemoryClientRepository();
        var tagRepo = new InMemoryTagRepository();
        var service = new ClientService(clientRepo, tagRepo, new FakeUnitOfWork());
        var client = Client.Create("Acme", null, null, null, null, (Industry)1, User, Creator);
        await clientRepo.AddAsync(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.SetStatusAsync(client.Id, 999));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowIfMissing()
    {
        var service = new ClientService(new InMemoryClientRepository(), new InMemoryTagRepository(), new FakeUnitOfWork());
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(Guid.NewGuid()));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
        public void Dispose() { }
    }

    private sealed class InMemoryClientRepository : IClientRepository
    {
        private readonly List<Client> _clients = [];
        public Task<Client?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_clients.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<Client>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Client>)_clients.ToList());

        public Task<IReadOnlyList<Client>> FindAsync(ISpecification<Client> specification, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Client>)_clients.ToList());

        public Task AddAsync(Client entity, CancellationToken ct = default)
        {
            _clients.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Client entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(Client entity, CancellationToken ct = default)
        {
            _clients.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Client>> GetByAssignedUserIdAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Client>)_clients.Where(x => x.AssignedUserId == userId).ToList());

        public Task<Client?> GetWithContactsAsync(Guid id, CancellationToken ct = default) =>
            GetByIdAsync(id, ct);
    }

    private sealed class InMemoryTagRepository : ITagRepository
    {
        private readonly List<Tag> _tags = [];

        public Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_tags.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Tag>)_tags.ToList());

        public Task<IReadOnlyList<Tag>> FindAsync(ISpecification<Tag> specification, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<Tag>)_tags.ToList());

        public Task AddAsync(Tag entity, CancellationToken ct = default)
        {
            _tags.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Tag entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(Tag entity, CancellationToken ct = default)
        {
            _tags.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default) =>
            Task.FromResult(_tags.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)));
    }

}
