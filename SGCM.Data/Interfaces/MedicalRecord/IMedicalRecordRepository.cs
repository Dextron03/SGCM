using SGCM.Domain.Core;
using SGCM.Domain.Entities;

namespace SGCM.Data.Interfaces
{
    public interface IMedicalRecordRepository : IBaseRepository<MedicalRecord>
    {
        Task<OperationResult> GetByPatient(string patientId);
        Task<OperationResult> GetByAppointment(string appointmentId);
    }
}
