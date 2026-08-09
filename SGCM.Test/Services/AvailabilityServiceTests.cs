using Moq;
using SGCM.Application.DTOs.Availability;
using SGCM.Application.Services;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;
using SGCM.Domain.Enums;
using Xunit;

namespace SGCM.Test.Services
{
    public class AvailabilityServiceTests
    {
        private static (Mock<IAvailabilityRepository> availabilityRepo, Mock<IDoctorRepository> doctorRepo, AvailabilityService service) CreateService()
        {
            var availabilityRepo = new Mock<IAvailabilityRepository>();
            var doctorRepo = new Mock<IDoctorRepository>();
            availabilityRepo.Setup(r => r.GetByDoctor(It.IsAny<string>())).ReturnsAsync(new OperationResult { Data = new List<Availability>() });
            var service = new AvailabilityService(availabilityRepo.Object, doctorRepo.Object);
            return (availabilityRepo, doctorRepo, service);
        }

        [Fact]
        public async Task Create_ValidDto_ShouldReturnSuccessWithDto()
        {
            // Arrange
            var (availabilityRepo, doctorRepo, service) = CreateService();
            doctorRepo.Setup(r => r.GetById("doc-1")).ReturnsAsync(new OperationResult { Data = new Doctor { Id = "doc-1" } });
            availabilityRepo.Setup(r => r.Add(It.IsAny<Availability>())).ReturnsAsync((Availability a) => new OperationResult { Data = a });
            var dto = new CreateAvailabilityDto { DoctorId = "doc-1", Day = AvailableDay.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.True(result.Success);
            var returned = (AvailabilityDto)result.Data!;
            Assert.Equal("doc-1", returned.DoctorId);
            Assert.Equal(AvailableDay.Monday, returned.Day);
        }

        [Fact]
        public async Task Create_DoctorDoesNotExist_ShouldFail()
        {
            // Arrange
            var (_, doctorRepo, service) = CreateService();
            doctorRepo.Setup(r => r.GetById("doc-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new CreateAvailabilityDto { DoctorId = "doc-x", Day = AvailableDay.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El doctor especificado no existe.", result.Message);
        }

        [Fact]
        public async Task Create_StartTimeAfterEndTime_ShouldFail()
        {
            // Arrange
            var (_, _, service) = CreateService();
            var dto = new CreateAvailabilityDto { DoctorId = "doc-1", Day = AvailableDay.Monday, StartTime = TimeSpan.FromHours(12), EndTime = TimeSpan.FromHours(8) };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("La hora de inicio debe ser anterior a la hora de fin.", result.Message);
        }

        [Fact]
        public async Task Update_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (availabilityRepo, _, service) = CreateService();
            availabilityRepo.Setup(r => r.GetById("a-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new UpdateAvailabilityDto { Id = "a-x", Day = AvailableDay.Tuesday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Registro no encontrado.", result.Message);
        }

        [Fact]
        public async Task Update_StartTimeAfterEndTime_ShouldFail()
        {
            // Arrange
            var (_, _, service) = CreateService();
            var dto = new UpdateAvailabilityDto { Id = "a-1", Day = AvailableDay.Tuesday, StartTime = TimeSpan.FromHours(12), EndTime = TimeSpan.FromHours(8) };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("La hora de inicio debe ser anterior a la hora de fin.", result.Message);
        }

        [Fact]
        public async Task Update_Valid_ShouldReturnUpdatedDto()
        {
            // Arrange
            var (availabilityRepo, _, service) = CreateService();
            var availability = new Availability { Id = "a-1", DoctorId = "doc-1", Day = AvailableDay.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) };
            availabilityRepo.Setup(r => r.GetById("a-1")).ReturnsAsync(new OperationResult { Data = availability });
            availabilityRepo.Setup(r => r.Update(It.IsAny<Availability>())).ReturnsAsync((Availability a) => new OperationResult { Data = a });
            var dto = new UpdateAvailabilityDto { Id = "a-1", Day = AvailableDay.Tuesday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(13) };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.True(result.Success);
            var returned = (AvailabilityDto)result.Data!;
            Assert.Equal(AvailableDay.Tuesday, returned.Day);
        }

        [Fact]
        public async Task Delete_EmptyId_ShouldFail()
        {
            // Arrange
            var (_, _, service) = CreateService();

            // Act
            var result = await service.Delete("");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El identificador de la disponibilidad no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task GetById_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var (availabilityRepo, _, service) = CreateService();
            var availability = new Availability { Id = "a-1", DoctorId = "doc-1", Day = AvailableDay.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) };
            availabilityRepo.Setup(r => r.GetById("a-1")).ReturnsAsync(new OperationResult { Data = availability });

            // Act
            var result = await service.GetById("a-1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("a-1", ((AvailabilityDto)result.Data!).Id);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedList()
        {
            // Arrange
            var (availabilityRepo, _, service) = CreateService();
            var availabilities = new List<Availability>
            {
                new Availability { Id = "a-1", DoctorId = "doc-1", Day = AvailableDay.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) }
            };
            availabilityRepo.Setup(r => r.GetAll()).ReturnsAsync(new OperationResult { Data = availabilities });

            // Act
            var result = await service.GetAll();

            // Assert
            Assert.True(result.Success);
            Assert.Single((List<AvailabilityDto>)result.Data!);
        }

        [Fact]
        public async Task GetByDoctor_ShouldReturnMappedList()
        {
            // Arrange
            var (availabilityRepo, _, service) = CreateService();
            var availabilities = new List<Availability>
            {
                new Availability { Id = "a-1", DoctorId = "doc-1", Day = AvailableDay.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) },
                new Availability { Id = "a-2", DoctorId = "doc-1", Day = AvailableDay.Tuesday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(12) }
            };
            availabilityRepo.Setup(r => r.GetByDoctor("doc-1")).ReturnsAsync(new OperationResult { Data = availabilities });

            // Act
            var result = await service.GetByDoctor("doc-1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, ((List<AvailabilityDto>)result.Data!).Count);
        }
    }
}
