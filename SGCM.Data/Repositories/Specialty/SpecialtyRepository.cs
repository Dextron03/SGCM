using SGCM.Data.Context;
using SGCM.Data.Interfaces;

namespace SGCM.Data.Repositories
{
    public class SpecialtyRepository : BaseRepository<Domain.Entities.Specialty>, ISpecialtyRepository
    {
        public SpecialtyRepository(SgcmDbContext context) : base(context)
        {
        }
    }
}
