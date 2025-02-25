using AutomationOfPurchases.Shared.Models;

namespace AutomationOfPurchases.API.Repositories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly AppDbContext _context;

        public RepositoryFactory(AppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<TEntity> CreateRepository<TEntity>() where TEntity : class
        {
            return new GenericRepository<TEntity>(_context);
        }
    }
}
