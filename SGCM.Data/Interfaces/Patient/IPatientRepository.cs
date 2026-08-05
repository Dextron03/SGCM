using SGCM.Domain.Core;
using SGCM.Domain.Entities;

namespace SGCM.Data.Interfaces
{
    public interface IPatientRepository : IBaseRepository<Patient>
    {
        Task<OperationResult> GetByAppUserId(string appUserId);
        Task<OperationResult> GetBySocialSecurityNumberAsync(string socialSecurityNumber);
    }
}
