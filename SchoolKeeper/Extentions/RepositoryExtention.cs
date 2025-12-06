using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SchoolKeeper.Abstractions.Interfaces.Repository;
using System.Linq.Expressions;

namespace SchoolKeeper.Extentions
{
    public static class RepositoryExtention
    {
        // Include от репозитория
        public static IIncludableQueryable<T, TProperty> Include<T, TProperty>(
            this IGenericRepository<T> repository,
            Expression<Func<T, TProperty>> navigationPropertyPath)
            where T : BaseModel
        {
            return EntityFrameworkQueryableExtensions.Include(repository.Query(), navigationPropertyPath);
        }

        // ThenInclude для цепочки вызовов (не конфликтует с EF Core, так как EF Core не имеет такого метода для IIncludableQueryable)
        public static IIncludableQueryable<T, TNextProperty> ThenInclude<T, TPreviousProperty, TNextProperty>(
            this IIncludableQueryable<T, TPreviousProperty> source,
            Expression<Func<TPreviousProperty, TNextProperty>> navigationPropertyPath)
            where T : BaseModel
        {
            return EntityFrameworkQueryableExtensions.ThenInclude(source, navigationPropertyPath);
        }

        public static async Task<T?> GetByIdAsync<T>(this IQueryable<T> query, int id) where T : BaseModel
        {
            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
