using SGCM.Application.DTOs.Availability;
using SGCM.Domain.Core;

namespace SGCM.Application.Interfaces
{
    public interface IAvailabilityService
    {
        Task<OperationResult> GetAll();
        Task<OperationResult> GetById(string id);
        Task<OperationResult> Create(CreateAvailabilityDto dto);
        Task<OperationResult> Update(UpdateAvailabilityDto dto);
        Task<OperationResult> Delete(string id);
        Task<OperationResult> GetByDoctor(string doctorId);
    }
}
