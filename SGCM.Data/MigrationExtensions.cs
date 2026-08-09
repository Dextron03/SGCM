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

        public static void SeedRoles(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            try
            {
                foreach (var role in AppRoles.All)
                {
                    if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                    }
                }
                Console.WriteLine("Roles verificados/creados con exito.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear roles: {ex.Message}");
            }
        }
    }
}