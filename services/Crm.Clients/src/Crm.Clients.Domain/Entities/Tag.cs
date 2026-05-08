using Crm.Shared.Domain;

namespace Crm.Clients.Domain.Entities;

public class Tag : AggregateRoot
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private Tag() { }

    public static Tag Create(string name, string? description = null, string? color = null)
    {
        return new Tag
        {
            Name = name,
            Description = description,
            Color = color,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description = null, string? color = null)
    {
        Name = name;
        Description = description;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }
}
