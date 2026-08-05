using SGCM.Data.Repositories;
using SGCM.Domain.Entities;
using SGCM.Domain.Enums;
using SGCM.Test.Common;
using Xunit;

namespace SGCM.Test.Repositories
{
    public class AppointmentRepositoryTests
    {
        private static Appointment NewAppointment(
            string patientId = "patient-1",
            string doctorId = "doctor-1",
            AppointmentStatus status = AppointmentStatus.Pending) => new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Status = status,
            DateTime = DateTime.UtcNow.AddDays(1),
            Reason = "Consulta general"
        };

        [Fact]
        public async Task GetByPatient_ShouldReturnOnlyPatientAppointments()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new AppointmentRepository(context);
            await repository.Add(NewAppointment(patientId: "patient-1"));
            await repository.Add(NewAppointment(patientId: "patient-1"));
            await repository.Add(NewAppointment(patientId: "patient-2"));

            var result = await repository.GetByPatient("patient-1");

            Assert.True(result.Success);
            var appointments = (List<Appointment>)result.Data!;
            Assert.Equal(2, appointments.Count);
            Assert.All(appointments, a => Assert.Equal("patient-1", a.PatientId));
        }

        [Fact]
        public async Task GetByDoctor_ShouldReturnOnlyDoctorAppointments()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new AppointmentRepository(context);
            await repository.Add(NewAppointment(doctorId: "doctor-1"));
            await repository.Add(NewAppointment(doctorId: "doctor-2"));

            var result = await repository.GetByDoctor("doctor-2");

            Assert.True(result.Success);
            var appointments = (List<Appointment>)result.Data!;
            Assert.Single(appointments);
            Assert.Equal("doctor-2", appointments[0].DoctorId);
        }

        [Fact]
        public async Task GetByStatus_ShouldFilterByStatus()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new AppointmentRepository(context);
            await repository.Add(NewAppointment(status: AppointmentStatus.Pending));
            await repository.Add(NewAppointment(status: AppointmentStatus.Confirmed));
            await repository.Add(NewAppointment(status: AppointmentStatus.Confirmed));

            var result = await repository.GetByStatus(AppointmentStatus.Confirmed);

            Assert.True(result.Success);
            var appointments = (List<Appointment>)result.Data!;
            Assert.Equal(2, appointments.Count);
            Assert.All(appointments, a => Assert.Equal(AppointmentStatus.Confirmed, a.Status));
        }

        [Fact]
        public async Task ChangeStatus_WhenAppointmentExists_ShouldUpdateStatus()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new AppointmentRepository(context);
            var appointment = NewAppointment(status: AppointmentStatus.Pending);
            await repository.Add(appointment);

            var result = await repository.ChangeStatus(appointment.Id, AppointmentStatus.Canceled);

            Assert.True(result.Success);
            var updated = context.Appointments.Single(a => a.Id == appointment.Id);
            Assert.Equal(AppointmentStatus.Canceled, updated.Status);
        }

        [Fact]
        public async Task ChangeStatus_WhenAppointmentDoesNotExist_ShouldFail()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new AppointmentRepository(context);

            var result = await repository.ChangeStatus("id-inexistente", AppointmentStatus.Confirmed);

            Assert.False(result.Success);
            Assert.Equal("Cita no encontrada.", result.Message);
        }
    }
}
