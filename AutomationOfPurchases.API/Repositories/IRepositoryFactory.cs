namespace AutomationOfPurchases.API.Repositories
{
    public interface IRepositoryFactory
    {
        IGenericRepository<TEntity> CreateRepository<TEntity>() where TEntity : class;
    }
}
