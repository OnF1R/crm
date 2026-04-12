namespace Crm.Shared.Domain;

public interface ISpecification<T>
{
    IQueryable<T> Apply(IQueryable<T> query);
}
