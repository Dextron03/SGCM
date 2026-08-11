using Microsoft.AspNetCore.Identity;
using Moq;
using SGCM.Application.DTOs.Appointment;
using SGCM.Application.DTOs.Availability;
using SGCM.Application.Services;
using SGCM.Data.Context;
using SGCM.Data.Interfaces;
using SGCM.Data.Repositories;
using SGCM.Domain.Entities;
using SGCM.Domain.Enums;
using SGCM.Test.Common;
using Xunit;

namespace SGCM.Test.Integration;

public class SchedulingInMemoryTests
{
    [Fact]
    public async Task AvailabilityCrud_ShouldPersistAndRejectOverlappingBlocks()
    {
        using var context = TestDbContextFactory.Create();
        await SeedProfiles(context);
        var service = CreateAvailabilityService(context);

        var created = await service.Create(new CreateAvailabilityDto
        {
            DoctorId = "doctor-1",
            Day = AvailableDay.Monday,
            StartTime = TimeSpan.FromHours(8),
            EndTime = TimeSpan.FromHours(12)
        });

        Assert.True(created.Success);
        var availability = (AvailabilityDto)created.Data!;
        Assert.Single(context.Availabilities);

        var overlapping = await service.Create(new CreateAvailabilityDto
        {
            DoctorId = "doctor-1",
            Day = AvailableDay.Monday,
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(13)
        });
        Assert.False(overlapping.Success);
        Assert.Equal("El horario se superpone con otro bloque de disponibilidad.", overlapping.Message);

        var updated = await service.Update(new UpdateAvailabilityDto
        {
            Id = availability.Id,
            Day = AvailableDay.Tuesday,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(12)
        });
        Assert.True(updated.Success);
        Assert.Equal(AvailableDay.Tuesday, context.Availabilities.Single().Day);

        var deleted = await service.Delete(availability.Id);
        Assert.True(deleted.Success);
        Assert.Empty(context.Availabilities);
        Assert.Empty((List<AvailabilityDto>)(await service.GetByDoctor("doctor-1")).Data!);
    }

    [Fact]
    public async Task AppointmentCreation_ShouldRequireAvailabilityAndPreventDoubleBooking()
    {
        using var context = TestDbContextFactory.Create();
        await SeedProfiles(context);
        var availabilityService = CreateAvailabilityService(context);
        var appointmentService = CreateAppointmentService(context);
        var appointmentTime = NextMondayAt(9);

        Assert.True((await availabilityService.Create(new CreateAvailabilityDto
        {
            DoctorId = "doctor-1",
            Day = AvailableDay.Monday,
            StartTime = TimeSpan.FromHours(8),
            EndTime = TimeSpan.FromHours(12)
        })).Success);

        var created = await appointmentService.Create(new CreateAppointmentDto
        {
            PatientId = "patient-1",
            DoctorId = "doctor-1",
            DateTime = appointmentTime,
            Reason = "Consulta general"
        });
        Assert.True(created.Success);
        Assert.Equal(AppointmentStatus.Pending, ((AppointmentDto)created.Data!).Status);
        Assert.Single(context.Appointments);

        var duplicate = await appointmentService.Create(new CreateAppointmentDto
        {
            PatientId = "patient-2",
            DoctorId = "doctor-1",
            DateTime = appointmentTime,
            Reason = "Cita duplicada"
        });
        Assert.False(duplicate.Success);
        Assert.Equal("El horario seleccionado ya está ocupado.", duplicate.Message);

        var outsideSchedule = await appointmentService.Create(new CreateAppointmentDto
        {
            PatientId = "patient-2",
            DoctorId = "doctor-1",
            DateTime = appointmentTime.Date.AddHours(15),
            Reason = "Fuera de horario"
        });
        Assert.False(outsideSchedule.Success);
        Assert.Equal("El doctor no está disponible en la fecha y hora seleccionadas.", outsideSchedule.Message);

        var missingPatient = await appointmentService.Create(new CreateAppointmentDto
        {
            PatientId = "patient-missing",
            DoctorId = "doctor-1",
            DateTime = appointmentTime.AddHours(1),
            Reason = "Paciente inexistente"
        });
        Assert.False(missingPatient.Success);
        Assert.Equal("El paciente especificado no existe.", missingPatient.Message);
    }

    [Fact]
    public async Task AppointmentStatus_ShouldProtectTerminalStatesAndReleaseCanceledSlot()
    {
        using var context = TestDbContextFactory.Create();
        await SeedProfiles(context);
        var availabilityService = CreateAvailabilityService(context);
        var appointmentService = CreateAppointmentService(context);
        var appointmentTime = NextMondayAt(10);
        await availabilityService.Create(new CreateAvailabilityDto
        {
            DoctorId = "doctor-1",
            Day = AvailableDay.Monday,
            StartTime = TimeSpan.FromHours(8),
            EndTime = TimeSpan.FromHours(12)
        });

        var created = await appointmentService.Create(new CreateAppointmentDto
        {
            PatientId = "patient-1", DoctorId = "doctor-1", DateTime = appointmentTime, Reason = "Control"
        });
        var appointmentId = ((AppointmentDto)created.Data!).Id;

        var canceled = await appointmentService.ChangeStatus(new ChangeAppointmentStatusDto { Id = appointmentId, Status = AppointmentStatus.Canceled });
        Assert.True(canceled.Success);

        var rebooked = await appointmentService.Create(new CreateAppointmentDto
        {
            PatientId = "patient-2", DoctorId = "doctor-1", DateTime = appointmentTime, Reason = "Nuevo paciente"
        });
        Assert.True(rebooked.Success);

        var invalidTransition = await appointmentService.ChangeStatus(new ChangeAppointmentStatusDto
        {
            Id = appointmentId, Status = AppointmentStatus.Confirmed
        });
        Assert.False(invalidTransition.Success);
        Assert.Equal("No se permite cambiar la cita a ese estado.", invalidTransition.Message);
    }

    [Fact]
    public async Task AppointmentUpdate_ShouldRejectAnOccupiedOrUnavailableTime()
    {
        using var context = TestDbContextFactory.Create();
        await SeedProfiles(context);
        var availabilityService = CreateAvailabilityService(context);
        var appointmentService = CreateAppointmentService(context);
        var firstTime = NextMondayAt(8);
        await availabilityService.Create(new CreateAvailabilityDto
        {
            DoctorId = "doctor-1", Day = AvailableDay.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12)
        });

        var first = await appointmentService.Create(new CreateAppointmentDto { PatientId = "patient-1", DoctorId = "doctor-1", DateTime = firstTime, Reason = "Primera" });
        var second = await appointmentService.Create(new CreateAppointmentDto { PatientId = "patient-2", DoctorId = "doctor-1", DateTime = firstTime.AddHours(1), Reason = "Segunda" });

        var collision = await appointmentService.Update(new UpdateAppointmentDto
        {
            Id = ((AppointmentDto)second.Data!).Id, DateTime = firstTime, Reason = "Intenta colisionar"
        });
        Assert.False(collision.Success);
        Assert.Equal("El horario seleccionado ya está ocupado.", collision.Message);

        var unavailable = await appointmentService.Update(new UpdateAppointmentDto
        {
            Id = ((AppointmentDto)first.Data!).Id, DateTime = firstTime.AddHours(7), Reason = "Fuera de horario"
        });
        Assert.False(unavailable.Success);
        Assert.Equal("El doctor no está disponible en la fecha y hora seleccionadas.", unavailable.Message);
    }

    private static AvailabilityService CreateAvailabilityService(SgcmDbContext context) =>
        new(new AvailabilityRepository(context), new DoctorRepository(context));

    private static AppointmentService CreateAppointmentService(SgcmDbContext context)
    {
        var store = new Mock<IUserStore<AppUser>>();
        var userManager = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        var emailSender = new Mock<IEmailSender>();

        return new AppointmentService(
            new AppointmentRepository(context),
            new DoctorRepository(context),
            new PatientRepository(context),
            new AvailabilityRepository(context),
            userManager.Object,
            emailSender.Object);
    }

    private static async Task SeedProfiles(SgcmDbContext context)
    {
        context.Doctors.Add(new Doctor { Id = "doctor-1", MedicalLicense = "MED-001", AppUserId = "doctor-user", SpecialtyId = "specialty-1" });
        context.Patients.AddRange(
            new Patient { Id = "patient-1", AppUserId = "patient-user-1", SocialSecurityNumber = "001", Address = "Dirección 1", DateOfBirth = new DateTime(1990, 1, 1) },
            new Patient { Id = "patient-2", AppUserId = "patient-user-2", SocialSecurityNumber = "002", Address = "Dirección 2", DateOfBirth = new DateTime(1992, 1, 1) });
        await context.SaveChangesAsync();
    }

    private static DateTime NextMondayAt(int hour)
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        return today.AddDays(daysUntilMonday).AddHours(hour);
    }
}
