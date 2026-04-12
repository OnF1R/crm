namespace Crm.Shared.Domain;

public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right) =>
        Equals(left, right);

    public static bool operator !=(Entity? left, Entity? right) =>
        !Equals(left, right);
}
