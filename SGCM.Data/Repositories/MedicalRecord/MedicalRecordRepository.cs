using Microsoft.EntityFrameworkCore;
using SGCM.Data.Context;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;

namespace SGCM.Data.Repositories
{
    public class MedicalRecordRepository : BaseRepository<Domain.Entities.MedicalRecord>, IMedicalRecordRepository
    {
        public MedicalRecordRepository(SgcmDbContext context) : base(context)
        {
        }

        public async Task<OperationResult> GetByPatient(string patientId)
        {
            var result = new OperationResult();
            try
            {
                result.Data = await _entities.AsNoTracking()
                    .Where(m => m.PatientId == patientId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener el historial medico del paciente: {ex.Message}";
            }
            return result;
        }

        public async Task<OperationResult> GetByAppointment(string appointmentId)
        {
            var result = new OperationResult();
            try
            {
                var record = await _entities.AsNoTracking()
                    .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId);
                if (record is null)
                {
                    result.Success = false;
                    result.Message = "Registro medico no encontrado para la cita.";
                    return result;
                }
                result.Data = record;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener el registro medico de la cita: {ex.Message}";
            }
            return result;
        }
    }
}
