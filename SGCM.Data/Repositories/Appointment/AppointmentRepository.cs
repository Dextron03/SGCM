using Microsoft.EntityFrameworkCore;
using SGCM.Data.Context;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Enums;

namespace SGCM.Data.Repositories
{
    public class AppointmentRepository : BaseRepository<Domain.Entities.Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(SgcmDbContext context) : base(context)
        {
        }

        public async Task<OperationResult> GetByPatient(string patientId)
        {
            var result = new OperationResult();
            try
            {
                result.Data = await _entities.AsNoTracking()
                    .Where(a => a.PatientId == patientId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener las citas del paciente: {ex.Message}";
            }
            return result;
        }

        public async Task<OperationResult> GetByDoctor(string doctorId)
        {
            var result = new OperationResult();
            try
            {
                result.Data = await _entities.AsNoTracking()
                    .Where(a => a.DoctorId == doctorId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener las citas del doctor: {ex.Message}";
            }
            return result;
        }

        public async Task<OperationResult> GetByStatus(AppointmentStatus status)
        {
            var result = new OperationResult();
            try
            {
                result.Data = await _entities.AsNoTracking()
                    .Where(a => a.Status == status)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener las citas por estado: {ex.Message}";
            }
            return result;
        }

        public async Task<OperationResult> ChangeStatus(string appointmentId, AppointmentStatus status)
        {
            var result = new OperationResult();
            try
            {
                var appointment = await _entities.FindAsync(appointmentId);
                if (appointment is null)
                {
                    result.Success = false;
                    result.Message = "Cita no encontrada.";
                    return result;
                }
                appointment.Status = status;
                await _context.SaveChangesAsync();
                result.Message = "Estado de la cita actualizado con exito.";
                result.Data = appointment;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al cambiar el estado de la cita: {ex.Message}";
            }
            return result;
        }
    }
}
