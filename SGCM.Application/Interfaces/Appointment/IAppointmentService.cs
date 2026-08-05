using SGCM.Application.DTOs.Appointment;
using SGCM.Domain.Core;
using SGCM.Domain.Enums;

namespace SGCM.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<OperationResult> GetAll();
        Task<OperationResult> GetById(string id);
        Task<OperationResult> Create(CreateAppointmentDto dto);
        Task<OperationResult> Update(UpdateAppointmentDto dto);
        Task<OperationResult> Delete(string id);
        Task<OperationResult> GetByPatient(string patientId);
        Task<OperationResult> GetByDoctor(string doctorId);
        Task<OperationResult> GetByStatus(AppointmentStatus status);
        Task<OperationResult> ChangeStatus(ChangeAppointmentStatusDto dto);
    }
}
