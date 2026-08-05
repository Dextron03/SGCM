using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SGCM.Data.Context;
using SGCM.Data.Interfaces;
using SGCM.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGCM.Data
{
    public static class ServicesRegistration
    {
        public static void AddDataLayerIoc(this IServiceCollection service, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("DatabaseConfig:UseInMemoryDatabase"))
            {
                service.AddDbContext<SgcmDbContext>(options
                    => options.UseInMemoryDatabase("SGCM"));
            }
            else 
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                service.AddDbContext<SgcmDbContext>(
                    (serviceProvider, opt) =>
                    {
                        opt.EnableSensitiveDataLogging();
                        opt.UseSqlServer(connectionString,
                        m => m.MigrationsAssembly(typeof(SgcmDbContext).Assembly.FullName));
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                );
            }

            // Registro de repositorios
            service.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            service.AddScoped<IAppointmentRepository, AppointmentRepository>();
            service.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
            service.AddScoped<IDoctorRepository, DoctorRepository>();
            service.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
            service.AddScoped<IPatientRepository, PatientRepository>();
            service.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
        }

    }
}
