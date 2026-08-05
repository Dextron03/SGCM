using Moq;
using SGCM.Application.DTOs.MedicalRecord;
using SGCM.Application.Services;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;
using Xunit;

namespace SGCM.Test.Services
{
    public class MedicalRecordServiceTests
    {
        private static (
            Mock<IMedicalRecordRepository> medicalRecordRepo,
            Mock<IPatientRepository> patientRepo,
            Mock<IDoctorRepository> doctorRepo,
            Mock<IAppointmentRepository> appointmentRepo,
            MedicalRecordService service) CreateService()
        {
            var medicalRecordRepo = new Mock<IMedicalRecordRepository>();
            var patientRepo = new Mock<IPatientRepository>();
            var doctorRepo = new Mock<IDoctorRepository>();
            var appointmentRepo = new Mock<IAppointmentRepository>();
            var service = new MedicalRecordService(medicalRecordRepo.Object, patientRepo.Object, doctorRepo.Object, appointmentRepo.Object);
            return (medicalRecordRepo, patientRepo, doctorRepo, appointmentRepo, service);
        }

        [Fact]
        public async Task Create_ValidDto_ShouldReturnSuccessWithDto()
        {
            // Arrange
            var (medicalRecordRepo, patientRepo, doctorRepo, appointmentRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetById("pat-1")).ReturnsAsync(new OperationResult { Data = new Patient { Id = "pat-1" } });
            doctorRepo.Setup(r => r.GetById("doc-1")).ReturnsAsync(new OperationResult { Data = new Doctor { Id = "doc-1" } });
            appointmentRepo.Setup(r => r.GetById("app-1")).ReturnsAsync(new OperationResult { Data = new Appointment { Id = "app-1" } });
            medicalRecordRepo.Setup(r => r.GetByAppointment("app-1")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro medico no encontrado para la cita." });
            medicalRecordRepo.Setup(r => r.Add(It.IsAny<MedicalRecord>())).ReturnsAsync((MedicalRecord m) => new OperationResult { Data = m });
            var dto = new CreateMedicalRecordDto { Diagnosis = "Gripe", Treatment = "Reposo", Notes = "N/A", PatientId = "pat-1", DoctorId = "doc-1", AppointmentId = "app-1" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Gripe", ((MedicalRecordDto)result.Data!).Diagnosis);
        }

        [Fact]
        public async Task Create_PatientDoesNotExist_ShouldFail()
        {
            // Arrange
            var (_, patientRepo, _, _, service) = CreateService();
            patientRepo.Setup(r => r.GetById("pat-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new CreateMedicalRecordDto { Diagnosis = "Gripe", Treatment = "Reposo", PatientId = "pat-x", DoctorId = "doc-1", AppointmentId = "app-1" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El paciente especificado no existe.", result.Message);
        }

        [Fact]
        public async Task Create_DoctorDoesNotExist_ShouldFail()
        {
            // Arrange
            var (_, patientRepo, doctorRepo, _, service) = CreateService();
            patientRepo.Setup(r => r.GetById("pat-1")).ReturnsAsync(new OperationResult { Data = new Patient { Id = "pat-1" } });
            doctorRepo.Setup(r => r.GetById("doc-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new CreateMedicalRecordDto { Diagnosis = "Gripe", Treatment = "Reposo", PatientId = "pat-1", DoctorId = "doc-x", AppointmentId = "app-1" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El doctor especificado no existe.", result.Message);
        }

        [Fact]
        public async Task Create_AppointmentDoesNotExist_ShouldFail()
        {
            // Arrange
            var (_, patientRepo, doctorRepo, appointmentRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetById("pat-1")).ReturnsAsync(new OperationResult { Data = new Patient { Id = "pat-1" } });
            doctorRepo.Setup(r => r.GetById("doc-1")).ReturnsAsync(new OperationResult { Data = new Doctor { Id = "doc-1" } });
            appointmentRepo.Setup(r => r.GetById("app-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new CreateMedicalRecordDto { Diagnosis = "Gripe", Treatment = "Reposo", PatientId = "pat-1", DoctorId = "doc-1", AppointmentId = "app-x" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("La cita especificada no existe.", result.Message);
        }

        [Fact]
        public async Task Create_AppointmentAlreadyHasRecord_ShouldFail()
        {
            // Arrange
            var (medicalRecordRepo, patientRepo, doctorRepo, appointmentRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetById("pat-1")).ReturnsAsync(new OperationResult { Data = new Patient { Id = "pat-1" } });
            doctorRepo.Setup(r => r.GetById("doc-1")).ReturnsAsync(new OperationResult { Data = new Doctor { Id = "doc-1" } });
            appointmentRepo.Setup(r => r.GetById("app-1")).ReturnsAsync(new OperationResult { Data = new Appointment { Id = "app-1" } });
            medicalRecordRepo.Setup(r => r.GetByAppointment("app-1")).ReturnsAsync(new OperationResult { Data = new MedicalRecord { Id = "mr-1", AppointmentId = "app-1" } });
            var dto = new CreateMedicalRecordDto { Diagnosis = "Gripe", Treatment = "Reposo", PatientId = "pat-1", DoctorId = "doc-1", AppointmentId = "app-1" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("La cita ya tiene un registro médico asociado.", result.Message);
        }

        [Fact]
        public async Task Create_EmptyDiagnosis_ShouldFail()
        {
            // Arrange
            var (_, _, _, _, service) = CreateService();
            var dto = new CreateMedicalRecordDto { Diagnosis = "", Treatment = "Reposo", PatientId = "pat-1", DoctorId = "doc-1", AppointmentId = "app-1" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El diagnóstico no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task Update_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (medicalRecordRepo, _, _, _, service) = CreateService();
            medicalRecordRepo.Setup(r => r.GetById("mr-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new UpdateMedicalRecordDto { Id = "mr-x", Diagnosis = "Gripe", Treatment = "Reposo" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Registro no encontrado.", result.Message);
        }

        [Fact]
        public async Task Update_Valid_ShouldReturnUpdatedDto()
        {
            // Arrange
            var (medicalRecordRepo, _, _, _, service) = CreateService();
            var record = new MedicalRecord { Id = "mr-1", Diagnosis = "Gripe", Treatment = "Reposo", PatientId = "pat-1", DoctorId = "doc-1", AppointmentId = "app-1" };
            medicalRecordRepo.Setup(r => r.GetById("mr-1")).ReturnsAsync(new OperationResult { Data = record });
            medicalRecordRepo.Setup(r => r.Update(It.IsAny<MedicalRecord>())).ReturnsAsync((MedicalRecord m) => new OperationResult { Data = m });
            var dto = new UpdateMedicalRecordDto { Id = "mr-1", Diagnosis = "Gripe severa", Treatment = "Reposo y medicamento", Notes = "Actualizado" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Gripe severa", ((MedicalRecordDto)result.Data!).Diagnosis);
        }

        [Fact]
        public async Task Delete_EmptyId_ShouldFail()
        {
            // Arrange
            var (_, _, _, _, service) = CreateService();

            // Act
            var result = await service.Delete("");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El identificador del registro médico no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task GetByPatient_ShouldReturnMappedList()
        {
            // Arrange
            var (medicalRecordRepo, _, _, _, service) = CreateService();
            var records = new List<MedicalRecord> { new MedicalRecord { Id = "mr-1", PatientId = "pat-1", DoctorId = "doc-1", AppointmentId = "app-1" } };
            medicalRecordRepo.Setup(r => r.GetByPatient("pat-1")).ReturnsAsync(new OperationResult { Data = records });

            // Act
            var result = await service.GetByPatient("pat-1");

            // Assert
            Assert.True(result.Success);
            Assert.Single((List<MedicalRecordDto>)result.Data!);
        }

        [Fact]
        public async Task GetByAppointment_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var (medicalRecordRepo, _, _, _, service) = CreateService();
            var record = new MedicalRecord { Id = "mr-1", PatientId = "pat-1", DoctorId = "doc-1", AppointmentId = "app-1" };
            medicalRecordRepo.Setup(r => r.GetByAppointment("app-1")).ReturnsAsync(new OperationResult { Data = record });

            // Act
            var result = await service.GetByAppointment("app-1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("mr-1", ((MedicalRecordDto)result.Data!).Id);
        }

        [Fact]
        public async Task GetByAppointment_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (medicalRecordRepo, _, _, _, service) = CreateService();
            medicalRecordRepo.Setup(r => r.GetByAppointment("app-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro medico no encontrado para la cita." });

            // Act
            var result = await service.GetByAppointment("app-x");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Registro medico no encontrado para la cita.", result.Message);
        }
    }
}
