using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SGCM.Data.Context;
using SGCM.Domain.Constants;
using SGCM.Domain.Entities;
using SGCM.Domain.Enums;

namespace SGCM.Data
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            var runMigrations = Environment.GetEnvironmentVariable("RUN_MIGRATIONS");

            if(runMigrations != "true") return;

            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using SgcmDbContext context = scope.ServiceProvider.GetRequiredService<SgcmDbContext>();

            try
            {

                Console.WriteLine("Aplicando migraciones.");
                context.Database.Migrate();
                Console.WriteLine("Migraciones aplicadas con exito.");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error al migrar: {ex.Message}");
            }
        }

    }
}
