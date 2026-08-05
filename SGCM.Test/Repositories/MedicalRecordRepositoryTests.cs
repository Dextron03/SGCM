using SGCM.Data.Repositories;
using SGCM.Domain.Entities;
using SGCM.Test.Common;
using Xunit;

namespace SGCM.Test.Repositories
{
    public class MedicalRecordRepositoryTests
    {
        private static MedicalRecord NewMedicalRecord(
            string patientId = "patient-1",
            string appointmentId = "appointment-1") => new MedicalRecord
        {
            Diagnosis = "Gripe comun",
            Treatment = "Reposo e hidratacion",
            Notes = "Volver si hay fiebre",
            PatientId = patientId,
            DoctorId = "doctor-1",
            AppointmentId = appointmentId
        };

        [Fact]
        public async Task GetByPatient_ShouldReturnOnlyPatientRecords()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new MedicalRecordRepository(context);
            await repository.Add(NewMedicalRecord(patientId: "patient-1", appointmentId: "app-1"));
            await repository.Add(NewMedicalRecord(patientId: "patient-1", appointmentId: "app-2"));
            await repository.Add(NewMedicalRecord(patientId: "patient-2", appointmentId: "app-3"));

            var result = await repository.GetByPatient("patient-1");

            Assert.True(result.Success);
            var records = (List<MedicalRecord>)result.Data!;
            Assert.Equal(2, records.Count);
            Assert.All(records, r => Assert.Equal("patient-1", r.PatientId));
        }

        [Fact]
        public async Task GetByAppointment_WhenRecordExists_ShouldReturnIt()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new MedicalRecordRepository(context);
            var record = NewMedicalRecord(appointmentId: "app-9");
            await repository.Add(record);

            var result = await repository.GetByAppointment("app-9");

            Assert.True(result.Success);
            var found = (MedicalRecord)result.Data!;
            Assert.Equal(record.Id, found.Id);
        }

        [Fact]
        public async Task GetByAppointment_WhenRecordDoesNotExist_ShouldFail()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new MedicalRecordRepository(context);

            var result = await repository.GetByAppointment("app-inexistente");

            Assert.False(result.Success);
            Assert.Equal("Registro medico no encontrado para la cita.", result.Message);
        }
    }
}
