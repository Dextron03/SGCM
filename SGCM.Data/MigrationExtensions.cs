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

        public static async Task SeedTestDataAsync(this IApplicationBuilder app)
        {
            var runSeed = Environment.GetEnvironmentVariable("RUN_SEED");

            if (runSeed != "true") return;

            using IServiceScope scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<SgcmDbContext>();

            try
            {
                Console.WriteLine("Sembrando datos de prueba.");

                foreach (var role in AppRoles.All)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole(role));
                }

                await CreateTestUserAsync(userManager, "admin@sgcm.com", "Admin123!", "Administrador de Prueba", AppRoles.Admin);

                var doctorUser = await CreateTestUserAsync(userManager, "doctor@sgcm.com", "Doctor123!", "Doctor de Prueba", AppRoles.Doctor);
                if (doctorUser is not null && !context.Doctors.Any(d => d.AppUserId == doctorUser.Id))
                {
                    var specialty = context.Specialties.FirstOrDefault();
                    if (specialty is null)
                    {
                        specialty = new Specialty { Name = "Medicina General", Description = "Especialidad general de prueba" };
                        context.Specialties.Add(specialty);
                        await context.SaveChangesAsync();
                    }

                    context.Doctors.Add(new Doctor
                    {
                        AppUserId = doctorUser.Id,
                        SpecialtyId = specialty.Id,
                        MedicalLicense = "TEST-0001"
                    });
                }

                var patientUser = await CreateTestUserAsync(userManager, "patient@sgcm.com", "Patient123!", "Paciente de Prueba", AppRoles.Patient);
                if (patientUser is not null && !context.Patients.Any(p => p.AppUserId == patientUser.Id))
                {
                    context.Patients.Add(new Patient
                    {
                        AppUserId = patientUser.Id,
                        SocialSecurityNumber = "000-0000000-0",
                        DateOfBirth = new DateTime(1995, 1, 1),
                        Address = "Direccion de prueba"
                    });
                }

                await context.SaveChangesAsync();

                Console.WriteLine("Datos de prueba sembrados con exito.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al sembrar datos de prueba: {ex.Message}");
            }
        }

        private static async Task<AppUser?> CreateTestUserAsync(
            UserManager<AppUser> userManager, string email, string password, string fullName, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null) return existing;

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                Console.WriteLine($"No se pudo crear el usuario de prueba {email}: {string.Join(" ", result.Errors.Select(e => e.Description))}");
                return null;
            }

            await userManager.AddToRoleAsync(user, role);
            return user;
        }

    }
}
