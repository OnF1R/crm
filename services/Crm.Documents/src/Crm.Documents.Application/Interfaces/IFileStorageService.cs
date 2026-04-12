namespace Crm.Documents.Application.Interfaces;

public interface IFileStorageService
{
    Task SaveAsync(Stream stream, string fileName);
    Task<Stream?> GetAsync(string fileName);
    Task DeleteAsync(string fileName);
}
