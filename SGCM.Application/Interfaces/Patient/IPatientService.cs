using SGCM.Application.DTOs.Patient;
using SGCM.Domain.Core;

namespace SGCM.Application.Interfaces
{
    public interface IPatientService
    {
        Task<OperationResult> GetAll();
        Task<OperationResult> GetById(string id);
        Task<OperationResult> Create(CreatePatientDto dto);
        Task<OperationResult> Update(UpdatePatientDto dto);
        Task<OperationResult> Delete(string id);
        Task<OperationResult> GetByAppUserId(string appUserId);
        Task<OperationResult> GetBySocialSecurityNumber(string socialSecurityNumber);
    }
}
