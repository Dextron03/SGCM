using SGCM.Application.DTOs.Specialty;
using SGCM.Domain.Core;

namespace SGCM.Application.Interfaces
{
    public interface ISpecialtyService
    {
        Task<OperationResult> GetAll();
        Task<OperationResult> GetById(string id);
        Task<OperationResult> Create(CreateSpecialtyDto dto);
        Task<OperationResult> Update(UpdateSpecialtyDto dto);
        Task<OperationResult> Delete(string id);
    }
}
