using Microsoft.EntityFrameworkCore;
using SGCM.Data.Context;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;

namespace SGCM.Data.Repositories
{
    public class DoctorRepository : BaseRepository<Domain.Entities.Doctor>, IDoctorRepository
    {
        public DoctorRepository(SgcmDbContext context) : base(context)
        {
        }

        public async Task<OperationResult> GetByAppUserId(string appUserId)
        {
            var result = new OperationResult();
            try
            {
                var doctor = await _entities.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.AppUserId == appUserId);
                if (doctor is null)
                {
                    result.Success = false;
                    result.Message = "Doctor no encontrado.";
                    return result;
                }
                result.Data = doctor;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener el doctor: {ex.Message}";
            }
            return result;
        }

        public async Task<OperationResult> GetBySpecialty(string specialtyId)
        {
            var result = new OperationResult();
            try
            {
                result.Data = await _entities.AsNoTracking()
                    .Where(d => d.SpecialtyId == specialtyId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener los doctores por especialidad: {ex.Message}";
            }
            return result;
        }

        public async Task<OperationResult> GetByMedicalLicenseAsync(string medicalLicense)
        {
            if (string.IsNullOrWhiteSpace(medicalLicense))
                return new OperationResult { Success = false, Message = "La licencia médica no puede estar vacía." };

            var doctor = await _entities.FirstOrDefaultAsync(d => d.MedicalLicense == medicalLicense);
            if (doctor == null)
                return new OperationResult { Success = false, Message = "No existe un doctor con esa licencia." };

            return new OperationResult { Data = doctor };
        }
    }
}
