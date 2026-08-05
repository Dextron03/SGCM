using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SGCM.Data.Context;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;

namespace SGCM.Data.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected readonly SgcmDbContext _context;
        protected readonly DbSet<TEntity> _entities;

        public BaseRepository(SgcmDbContext context)
        {
            _context = context;
            _entities = context.Set<TEntity>();
        }

        public virtual async Task<OperationResult> GetAll()
        {
            var result = new OperationResult();
            try
            {
                result.Data = await _entities.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener los registros: {ex.Message}";
            }
            return result;
        }

        public virtual async Task<OperationResult> GetById(string id)
        {
            var result = new OperationResult();
            try
            {
                var entity = await _entities.FindAsync(id);
                if (entity is null)
                {
                    result.Success = false;
                    result.Message = "Registro no encontrado.";
                    return result;
                }
                result.Data = entity;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener el registro: {ex.Message}";
            }
            return result;
        }

        public virtual async Task<OperationResult> Add(TEntity entity)
        {
            var result = new OperationResult();
            try
            {
                await _entities.AddAsync(entity);
                await _context.SaveChangesAsync();
                result.Message = "Registro creado con exito.";
                result.Data = entity;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al crear el registro: {ex.Message}";
            }
            return result;
        }

        public virtual async Task<OperationResult> Update(TEntity entity)
        {
            var result = new OperationResult();
            try
            {
                _entities.Update(entity);
                await _context.SaveChangesAsync();
                result.Message = "Registro actualizado con exito.";
                result.Data = entity;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al actualizar el registro: {ex.Message}";
            }
            return result;
        }

        public virtual async Task<OperationResult> Delete(string id)
        {
            var result = new OperationResult();
            try
            {
                var entity = await _entities.FindAsync(id);
                if (entity is null)
                {
                    result.Success = false;
                    result.Message = "Registro no encontrado.";
                    return result;
                }
                _entities.Remove(entity);
                await _context.SaveChangesAsync();
                result.Message = "Registro eliminado con exito.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al eliminar el registro: {ex.Message}";
            }
            return result;
        }

        public virtual async Task<bool> Exists(Expression<Func<TEntity, bool>> predicate)
        {
            return await _entities.AnyAsync(predicate);
        }
    }
}
