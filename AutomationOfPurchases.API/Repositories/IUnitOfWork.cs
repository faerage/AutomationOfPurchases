using System;

namespace AutomationOfPurchases.API.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        // Приклад репозиторіїв для конкретних сутностей
        IGenericRepository<AutomationOfPurchases.Shared.Models.AppUser> Users { get; }
        IGenericRepository<AutomationOfPurchases.Shared.Models.Department> Departments { get; }
        IGenericRepository<AutomationOfPurchases.Shared.Models.Request> Requests { get; }
        // за потреби додати й інші, наприклад:
        // IGenericRepository<Item> Items { get; }
        // IGenericRepository<RequestItem> RequestItems { get; }

        // Універсальне створення репозиторіїв через Factory, якщо потрібно:
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

        // Основний метод збереження
        Task<int> SaveChangesAsync();
    }
}
