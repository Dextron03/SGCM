using SGCM.Application.DTOs.MedicalRecord;
using SGCM.Domain.Core;

namespace SGCM.Application.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<OperationResult> GetAll();
        Task<OperationResult> GetById(string id);
        Task<OperationResult> Create(CreateMedicalRecordDto dto);
        Task<OperationResult> Update(UpdateMedicalRecordDto dto);
        Task<OperationResult> Delete(string id);
        Task<OperationResult> GetByPatient(string patientId);
        Task<OperationResult> GetByAppointment(string appointmentId);
    }
}
