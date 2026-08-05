using Microsoft.Extensions.DependencyInjection;
using SGCM.Application.Interfaces;
using SGCM.Application.Services;

namespace SGCM.Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationIoc(this IServiceCollection services)
        {
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<ISpecialtyService, SpecialtyService>();
            services.AddScoped<IAvailabilityService, AvailabilityService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IMedicalRecordService, MedicalRecordService>();
        }
    }
}
