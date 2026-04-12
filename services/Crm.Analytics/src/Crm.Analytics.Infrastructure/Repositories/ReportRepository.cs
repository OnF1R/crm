using Crm.Analytics.Domain.Entities;
using Crm.Analytics.Domain.Interfaces;
using Crm.Analytics.Infrastructure.Persistence;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Crm.Analytics.Infrastructure.Repositories;

public class ReportRepository : Repository<Report>, IReportRepository
{
    public ReportRepository(AnalyticsDbContext dbContext) : base(dbContext) { }
}
