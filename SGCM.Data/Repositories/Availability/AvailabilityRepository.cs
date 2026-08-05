using Microsoft.EntityFrameworkCore;
using SGCM.Data.Context;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;

namespace SGCM.Data.Repositories
{
    public class AvailabilityRepository : BaseRepository<Domain.Entities.Availability>, IAvailabilityRepository
    {
        public AvailabilityRepository(SgcmDbContext context) : base(context)
        {
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
                result.Message = $"Error al obtener la disponibilidad del doctor: {ex.Message}";
            }
            return result;
        }
    }
}
