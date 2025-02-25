using AutomationOfPurchases.Shared.Models;

namespace AutomationOfPurchases.API.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IRepositoryFactory _repositoryFactory;

        // "кешовані" репозиторії
        private IGenericRepository<AppUser>? _users;
        private IGenericRepository<Department>? _departments;
        private IGenericRepository<Request>? _requests;
        // Додаткові поля для інших сутностей:
        // private IGenericRepository<Item>? _items;

        public UnitOfWork(AppDbContext context, IRepositoryFactory repositoryFactory)
        {
            _context = context;
            _repositoryFactory = repositoryFactory;
        }

        public IGenericRepository<AppUser> Users
            => _users ??= _repositoryFactory.CreateRepository<AppUser>();

        public IGenericRepository<Department> Departments
            => _departments ??= _repositoryFactory.CreateRepository<Department>();

        public IGenericRepository<Request> Requests
            => _requests ??= _repositoryFactory.CreateRepository<Request>();

        // Якщо потрібен доступ до інших сутностей:
        // public IGenericRepository<Item> Items => _items ??= _repositoryFactory.CreateRepository<Item>();

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            return _repositoryFactory.CreateRepository<TEntity>();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
