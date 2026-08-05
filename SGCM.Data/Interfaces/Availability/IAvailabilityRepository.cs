using SGCM.Domain.Core;
using SGCM.Domain.Entities;

namespace SGCM.Data.Interfaces
{
    public interface IAvailabilityRepository : IBaseRepository<Availability>
    {
        Task<OperationResult> GetByDoctor(string doctorId);
    }
}
