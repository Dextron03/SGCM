using Microsoft.EntityFrameworkCore;
using SGCM.Data.Context;

namespace SGCM.Test.Common
{
    public static class TestDbContextFactory
    {
        /// <summary>
        /// Crea un SgcmDbContext sobre una base de datos en memoria unica,
        /// para que cada test quede aislado de los demas.
        /// </summary>
        public static SgcmDbContext Create()
        {
            var options = new DbContextOptionsBuilder<SgcmDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new SgcmDbContext(options);
        }
    }
}
