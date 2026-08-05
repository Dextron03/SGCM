using System.Linq.Expressions;
using SGCM.Domain.Core;

namespace SGCM.Data.Interfaces
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task<OperationResult> GetAll();
        Task<OperationResult> GetById(string id);
        Task<OperationResult> Add(TEntity entity);
        Task<OperationResult> Update(TEntity entity);
        Task<OperationResult> Delete(string id);
        Task<bool> Exists(Expression<Func<TEntity, bool>> predicate);
    }
}
