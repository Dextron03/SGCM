using Microsoft.EntityFrameworkCore;
using SGCM.Data.Context;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;

namespace SGCM.Data.Repositories
{
    public class PatientRepository : BaseRepository<Domain.Entities.Patient>, IPatientRepository
    {
        public PatientRepository(SgcmDbContext context) : base(context)
        {
        }

        public async Task<OperationResult> GetByAppUserId(string appUserId)
        {
            var result = new OperationResult();
            try
            {
                var patient = await _entities.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.AppUserId == appUserId);
                if (patient is null)
                {
                    result.Success = false;
                    result.Message = "Paciente no encontrado.";
                    return result;
                }
                result.Data = patient;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error al obtener el paciente: {ex.Message}";
            }
            return result;
        }

        public async Task<OperationResult> GetBySocialSecurityNumberAsync(string socialSecurityNumber)
        {
            if (string.IsNullOrWhiteSpace(socialSecurityNumber))
                return new OperationResult { Success = false, Message = "El número de seguro social no puede estar vacío." };

            var patient = await _entities.FirstOrDefaultAsync(p => p.SocialSecurityNumber == socialSecurityNumber);
            if (patient == null)
                return new OperationResult { Success = false, Message = "No existe un paciente con ese número de seguro social." };

            return new OperationResult { Data = patient };
        }
    }
}
