using Moq;
using SGCM.Application.DTOs.Appointment;
using SGCM.Application.Services;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;
using SGCM.Domain.Enums;
using Xunit;

namespace SGCM.Test.Services
{
    public class AppointmentServiceTests
    {
        private static (Mock<IAppointmentRepository> appointmentRepo, Mock<IDoctorRepository> doctorRepo, Mock<IPatientRepository> patientRepo, AppointmentService service) CreateService()
        {
            var appointmentRepo = new Mock<IAppointmentRepository>();
            var doctorRepo = new Mock<IDoctorRepository>();
            var patientRepo = new Mock<IPatientRepository>();
            var availabilityRepo = new Mock<IAvailabilityRepository>();
            availabilityRepo.Setup(r => r.GetByDoctor(It.IsAny<string>())).ReturnsAsync(new OperationResult
            {
                Data = Enumerable.Range(1, 7).Select(day => new Availability
                {
                    Day = (AvailableDay)day,
                    StartTime = TimeSpan.Zero,
                    EndTime = TimeSpan.FromHours(24)
                }).ToList()
            });
            appointmentRepo.Setup(r => r.GetByDoctor(It.IsAny<string>())).ReturnsAsync(new OperationResult { Data = new List<Appointment>() });
            var service = new AppointmentService(appointmentRepo.Object, doctorRepo.Object, patientRepo.Object, availabilityRepo.Object);
            return (appointmentRepo, doctorRepo, patientRepo, service);
        }

        [Fact]
        public async Task Create_ValidDto_ShouldReturnSuccessWithDto()
        {
            // Arrange
            var (appointmentRepo, doctorRepo, patientRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetById("pat-1")).ReturnsAsync(new OperationResult { Data = new Patient { Id = "pat-1" } });
            doctorRepo.Setup(r => r.GetById("doc-1")).ReturnsAsync(new OperationResult { Data = new Doctor { Id = "doc-1" } });
            appointmentRepo.Setup(r => r.Add(It.IsAny<Appointment>())).ReturnsAsync((Appointment a) => new OperationResult { Data = a });
            var dto = new CreateAppointmentDto { PatientId = "pat-1", DoctorId = "doc-1", DateTime = DateTime.UtcNow.AddDays(1), Reason = "Chequeo" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.True(result.Success);
            var returned = (AppointmentDto)result.Data!;
            Assert.Equal(AppointmentStatus.Pending, returned.Status);
        }

        [Fact]
        public async Task Create_PatientDoesNotExist_ShouldFail()
        {
            // Arrange
            var (_, _, patientRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetById("pat-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new CreateAppointmentDto { PatientId = "pat-x", DoctorId = "doc-1", DateTime = DateTime.UtcNow.AddDays(1), Reason = "Chequeo" };

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
            var (_, doctorRepo, patientRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetById("pat-1")).ReturnsAsync(new OperationResult { Data = new Patient { Id = "pat-1" } });
            doctorRepo.Setup(r => r.GetById("doc-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new CreateAppointmentDto { PatientId = "pat-1", DoctorId = "doc-x", DateTime = DateTime.UtcNow.AddDays(1), Reason = "Chequeo" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El doctor especificado no existe.", result.Message);
        }

        [Fact]
        public async Task Create_PastDate_ShouldFail()
        {
            // Arrange
            var (_, _, _, service) = CreateService();
            var dto = new CreateAppointmentDto { PatientId = "pat-1", DoctorId = "doc-1", DateTime = DateTime.UtcNow.AddDays(-1), Reason = "Chequeo" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("La fecha de la cita no puede estar en el pasado.", result.Message);
        }

        [Fact]
        public async Task Create_EmptyReason_ShouldFail()
        {
            // Arrange
            var (_, _, _, service) = CreateService();
            var dto = new CreateAppointmentDto { PatientId = "pat-1", DoctorId = "doc-1", DateTime = DateTime.UtcNow.AddDays(1), Reason = "" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El motivo de la cita no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task Create_WithLocalDateTime_ShouldAcceptMatchingAvailability()
        {
            // Arrange
            var (appointmentRepo, doctorRepo, patientRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetById("pat-1")).ReturnsAsync(new OperationResult { Data = new Patient { Id = "pat-1" } });
            doctorRepo.Setup(r => r.GetById("doc-1")).ReturnsAsync(new OperationResult { Data = new Doctor { Id = "doc-1" } });
            appointmentRepo.Setup(r => r.Add(It.IsAny<Appointment>())).ReturnsAsync((Appointment a) => new OperationResult { Data = a });

            var localDateTime = new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Unspecified);
            var dto = new CreateAppointmentDto { PatientId = "pat-1", DoctorId = "doc-1", DateTime = localDateTime, Reason = "Chequeo" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task Update_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (appointmentRepo, _, _, service) = CreateService();
            appointmentRepo.Setup(r => r.GetById("app-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new UpdateAppointmentDto { Id = "app-x", DateTime = DateTime.UtcNow.AddDays(1), Reason = "Chequeo" };

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
            var (appointmentRepo, _, _, service) = CreateService();
            var appointment = new Appointment { Id = "app-1", PatientId = "pat-1", DoctorId = "doc-1", Reason = "Chequeo", DateTime = DateTime.UtcNow.AddDays(1) };
            appointmentRepo.Setup(r => r.GetById("app-1")).ReturnsAsync(new OperationResult { Data = appointment });
            appointmentRepo.Setup(r => r.Update(It.IsAny<Appointment>())).ReturnsAsync((Appointment a) => new OperationResult { Data = a });
            var newDate = DateTime.UtcNow.AddDays(2);
            var dto = new UpdateAppointmentDto { Id = "app-1", DateTime = newDate, Reason = "Seguimiento" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Seguimiento", ((AppointmentDto)result.Data!).Reason);
        }

        [Fact]
        public async Task Delete_EmptyId_ShouldFail()
        {
            // Arrange
            var (_, _, _, service) = CreateService();

            // Act
            var result = await service.Delete("");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El identificador de la cita no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task GetByPatient_ShouldReturnMappedList()
        {
            // Arrange
            var (appointmentRepo, _, _, service) = CreateService();
            var appointments = new List<Appointment> { new Appointment { Id = "app-1", PatientId = "pat-1", DoctorId = "doc-1", Reason = "Chequeo" } };
            appointmentRepo.Setup(r => r.GetByPatient("pat-1")).ReturnsAsync(new OperationResult { Data = appointments });

            // Act
            var result = await service.GetByPatient("pat-1");

            // Assert
            Assert.True(result.Success);
            Assert.Single((List<AppointmentDto>)result.Data!);
        }

        [Fact]
        public async Task GetByDoctor_ShouldReturnMappedList()
        {
            // Arrange
            var (appointmentRepo, _, _, service) = CreateService();
            var appointments = new List<Appointment> { new Appointment { Id = "app-1", PatientId = "pat-1", DoctorId = "doc-1", Reason = "Chequeo" } };
            appointmentRepo.Setup(r => r.GetByDoctor("doc-1")).ReturnsAsync(new OperationResult { Data = appointments });

            // Act
            var result = await service.GetByDoctor("doc-1");

            // Assert
            Assert.True(result.Success);
            Assert.Single((List<AppointmentDto>)result.Data!);
        }

        [Fact]
        public async Task GetByStatus_ShouldReturnMappedList()
        {
            // Arrange
            var (appointmentRepo, _, _, service) = CreateService();
            var appointments = new List<Appointment> { new Appointment { Id = "app-1", PatientId = "pat-1", DoctorId = "doc-1", Reason = "Chequeo", Status = AppointmentStatus.Confirmed } };
            appointmentRepo.Setup(r => r.GetByStatus(AppointmentStatus.Confirmed)).ReturnsAsync(new OperationResult { Data = appointments });

            // Act
            var result = await service.GetByStatus(AppointmentStatus.Confirmed);

            // Assert
            Assert.True(result.Success);
            Assert.Single((List<AppointmentDto>)result.Data!);
        }

        [Fact]
        public async Task ChangeStatus_EmptyId_ShouldFail()
        {
            // Arrange
            var (_, _, _, service) = CreateService();
            var dto = new ChangeAppointmentStatusDto { Id = "", Status = AppointmentStatus.Confirmed };

            // Act
            var result = await service.ChangeStatus(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El identificador de la cita no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task ChangeStatus_ShouldDelegateToRepository()
        {
            // Arrange
            var (appointmentRepo, _, _, service) = CreateService();
            var appointment = new Appointment { Id = "app-1", PatientId = "pat-1", DoctorId = "doc-1", Reason = "Chequeo", Status = AppointmentStatus.Confirmed };
            appointmentRepo.Setup(r => r.GetById("app-1")).ReturnsAsync(new OperationResult { Data = new Appointment { Id = "app-1", Status = AppointmentStatus.Pending } });
            appointmentRepo.Setup(r => r.ChangeStatus("app-1", AppointmentStatus.Confirmed)).ReturnsAsync(new OperationResult { Data = appointment });
            var dto = new ChangeAppointmentStatusDto { Id = "app-1", Status = AppointmentStatus.Confirmed };

            // Act
            var result = await service.ChangeStatus(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(AppointmentStatus.Confirmed, ((AppointmentDto)result.Data!).Status);
        }
    }
}
