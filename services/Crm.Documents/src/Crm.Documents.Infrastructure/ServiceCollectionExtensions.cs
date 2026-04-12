using Crm.Documents.Application.Interfaces;
using Crm.Documents.Application.Services;
using Crm.Documents.Domain.Entities;
using Crm.Documents.Domain.Interfaces;
using Crm.Documents.Infrastructure.Persistence;
using Crm.Documents.Infrastructure.Repositories;
using Crm.Documents.Infrastructure.Storage;
using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Documents.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DocumentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Documents")));
        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<DocumentsDbContext>());
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IRepository<Document>, Repository<Document>>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DocumentsDbContext>());
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddSingleton<IFileStorageService>(new LocalFileStorageService(
            Path.Combine(AppContext.BaseDirectory, "uploads")));
        return services;
    }
}
