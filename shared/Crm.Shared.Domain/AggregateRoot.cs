using System.ComponentModel.DataAnnotations.Schema;

namespace Crm.Shared.Domain;

public abstract class AggregateRoot : Entity
{
    [NotMapped]
    private readonly List<DomainEvent> _domainEvents = [];
    [NotMapped]
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() =>
        _domainEvents.Clear();
}
