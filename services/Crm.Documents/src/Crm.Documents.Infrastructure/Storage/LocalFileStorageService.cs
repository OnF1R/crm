using Crm.Documents.Application.Interfaces;

namespace Crm.Documents.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    public LocalFileStorageService(string storagePath)
    {
        _storagePath = storagePath;
        Directory.CreateDirectory(_storagePath);
    }

    public async Task SaveAsync(Stream stream, string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);
    }

    public Task<Stream?> GetAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        if (!File.Exists(filePath))
            return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(new FileStream(filePath, FileMode.Open));
    }

    public Task DeleteAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
        return Task.CompletedTask;
    }
}
