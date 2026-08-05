using SGCM.Application.DTOs.Doctor;
using SGCM.Domain.Core;

namespace SGCM.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<OperationResult> GetAll();
        Task<OperationResult> GetById(string id);
        Task<OperationResult> Create(CreateDoctorDto dto);
        Task<OperationResult> Update(UpdateDoctorDto dto);
        Task<OperationResult> Delete(string id);
        Task<OperationResult> GetByAppUserId(string appUserId);
        Task<OperationResult> GetBySpecialty(string specialtyId);
        Task<OperationResult> GetByMedicalLicense(string medicalLicense);
    }
}
