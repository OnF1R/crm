using Crm.Clients.Domain.Entities;
using Crm.Clients.Domain.Enums;
using Xunit;

namespace Crm.Clients.Tests;

public sealed class ClientDomainTests
{
    private static readonly Guid Creator = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Assignee = new("22222222-2222-2222-2222-222222222222");

    [Xunit.Fact]
    public void Create_ShouldInitializeStatusAndFields()
    {
        var created = Client.Create("Acme", "Big company", "https://x/logo.png", "1234567890", "https://acme.com", (Industry)1, Assignee, Creator);

        Assert.Equal("Acme", created.CompanyName);
        Assert.Equal("Big company", created.Description);
        Assert.Equal("https://x/logo.png", created.AvatarUrl);
        Assert.Equal("1234567890", created.Inn);
        Assert.Equal("https://acme.com", created.Website);
        Assert.Equal((Industry)1, created.Industry);
        Assert.Equal((Crm.Clients.Domain.Enums.ClientStatus)1, created.Status);
        Assert.Equal(Assignee, created.AssignedUserId);
        Assert.Empty(created.Contacts);
        Assert.Empty(created.Tags);
    }

    [Fact]
    public void AddContact_ShouldAttachContactToClient()
    {
        var client = Client.Create("Acme", null, null, null, null, (Industry)1, Assignee, Creator);
        var contact = client.AddContact("Ivan", "Ivanov", "ivan@acme.com", "+123", "Manager", true);

        Assert.Single(client.Contacts);
        Assert.Equal(client.Id, contact.ClientId);
        Assert.Equal("Ivan", contact.FirstName);
        Assert.True(contact.IsPrimary);
        Assert.Equal("Ivan Ivanov", contact.GetFullName());
    }

    [Fact]
    public void AddTag_ShouldIgnoreDuplicatesById()
    {
        var client = Client.Create("Acme", null, null, null, null, (Industry)1, Assignee, Creator);
        var tag = Tag.Create("VIP", "Important", "#fff");
        client.AddTag(tag);
        client.AddTag(tag);
        Assert.Single(client.Tags);
    }

    [Fact]
    public void RemoveTag_ShouldRemoveWhenExists()
    {
        var client = Client.Create("Acme", null, null, null, null, (Industry)1, Assignee, Creator);
        var tag = Tag.Create("VIP", "Important", "#fff");
        client.AddTag(tag);
        Assert.Single(client.Tags);

        client.RemoveTag(tag.Id);

        Assert.Empty(client.Tags);
    }

    [Fact]
    public void Contact_Update_ShouldChangeValues()
    {
        var contact = Contact.Create(Guid.NewGuid(), "Bob", "Doe", "bob@x.com", "+7", "Dev", false);
        contact.Update("Robert", "Doe", "robert@x.com", null, null, true);
        Assert.Equal("Robert", contact.FirstName);
        Assert.Equal("robert@x.com", contact.Email);
        Assert.Null(contact.Phone);
        Assert.True(contact.IsPrimary);
    }

    [Fact]
    public void Tag_Update_ShouldSetNewValuesAndTimestamp()
    {
        var tag = Tag.Create("VIP", "Important", "#fff");
        var before = DateTime.UtcNow;
        tag.Update("Priority", "Critical", "#000");
        Assert.Equal("Priority", tag.Name);
        Assert.Equal("Critical", tag.Description);
        Assert.Equal("#000", tag.Color);
        Assert.NotNull(tag.UpdatedAt);
        Assert.InRange(tag.UpdatedAt!.Value, before, DateTime.UtcNow);
    }
}
