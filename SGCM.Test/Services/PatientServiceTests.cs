using Moq;
using SGCM.Application.DTOs.Patient;
using SGCM.Application.Services;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;
using System.Linq.Expressions;
using Xunit;

namespace SGCM.Test.Services
{
    public class PatientServiceTests
    {
        private static (Mock<IPatientRepository> patientRepo, PatientService service) CreateService()
        {
            var patientRepo = new Mock<IPatientRepository>();
            var service = new PatientService(patientRepo.Object);
            return (patientRepo, service);
        }

        [Fact]
        public async Task Create_ValidDto_ShouldReturnSuccessWithDto()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            var dto = new CreatePatientDto
            {
                SocialSecurityNumber = "001-0000001-1",
                DateOfBirth = new DateTime(1990, 5, 20),
                Address = "Calle Principal #1",
                AppUserId = "user-1"
            };
            patientRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Patient, bool>>>())).ReturnsAsync(false);
            patientRepo.Setup(r => r.Add(It.IsAny<Patient>())).ReturnsAsync((Patient p) => new OperationResult { Data = p });

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("001-0000001-1", ((PatientDto)result.Data!).SocialSecurityNumber);
        }

        [Fact]
        public async Task Create_EmptySocialSecurityNumber_ShouldFail()
        {
            // Arrange
            var (_, service) = CreateService();
            var dto = new CreatePatientDto { SocialSecurityNumber = "", Address = "Calle 1", AppUserId = "user-1", DateOfBirth = new DateTime(1990, 1, 1) };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El número de seguro social no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task Create_DuplicateSocialSecurityNumber_ShouldFail()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            patientRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Patient, bool>>>())).ReturnsAsync(true);
            var dto = new CreatePatientDto { SocialSecurityNumber = "001-0000001-1", Address = "Calle 1", AppUserId = "user-1", DateOfBirth = new DateTime(1990, 1, 1) };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Ya existe un paciente con ese número de seguro social.", result.Message);
        }

        [Fact]
        public async Task Update_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetById("p-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new UpdatePatientDto { Id = "p-x", SocialSecurityNumber = "001-1", Address = "Calle 1" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Registro no encontrado.", result.Message);
        }

        [Fact]
        public async Task Update_DuplicateSocialSecurityNumberOnAnotherPatient_ShouldFail()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            var patient = new Patient { Id = "p-1", SocialSecurityNumber = "001-1", Address = "Calle 1", AppUserId = "user-1" };
            patientRepo.Setup(r => r.GetById("p-1")).ReturnsAsync(new OperationResult { Data = patient });
            patientRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Patient, bool>>>())).ReturnsAsync(true);
            var dto = new UpdatePatientDto { Id = "p-1", SocialSecurityNumber = "001-2", Address = "Calle 2", DateOfBirth = new DateTime(1990, 1, 1) };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Ya existe otro paciente con ese número de seguro social.", result.Message);
        }

        [Fact]
        public async Task Update_Valid_ShouldReturnUpdatedDto()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            var patient = new Patient { Id = "p-1", SocialSecurityNumber = "001-1", Address = "Calle 1", AppUserId = "user-1" };
            patientRepo.Setup(r => r.GetById("p-1")).ReturnsAsync(new OperationResult { Data = patient });
            patientRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Patient, bool>>>())).ReturnsAsync(false);
            patientRepo.Setup(r => r.Update(It.IsAny<Patient>())).ReturnsAsync((Patient p) => new OperationResult { Data = p });
            var dto = new UpdatePatientDto { Id = "p-1", SocialSecurityNumber = "001-2", Address = "Calle Nueva", DateOfBirth = new DateTime(1991, 1, 1) };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.True(result.Success);
            var returned = (PatientDto)result.Data!;
            Assert.Equal("001-2", returned.SocialSecurityNumber);
            Assert.Equal("Calle Nueva", returned.Address);
        }

        [Fact]
        public async Task Delete_EmptyId_ShouldFail()
        {
            // Arrange
            var (_, service) = CreateService();

            // Act
            var result = await service.Delete("");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El identificador del paciente no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task GetById_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            var patient = new Patient { Id = "p-1", SocialSecurityNumber = "001-1", Address = "Calle 1", AppUserId = "user-1" };
            patientRepo.Setup(r => r.GetById("p-1")).ReturnsAsync(new OperationResult { Data = patient });

            // Act
            var result = await service.GetById("p-1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("p-1", ((PatientDto)result.Data!).Id);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedList()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            var patients = new List<Patient>
            {
                new Patient { Id = "p-1", SocialSecurityNumber = "001-1", Address = "Calle 1", AppUserId = "user-1" },
                new Patient { Id = "p-2", SocialSecurityNumber = "001-2", Address = "Calle 2", AppUserId = "user-2" }
            };
            patientRepo.Setup(r => r.GetAll()).ReturnsAsync(new OperationResult { Data = patients });

            // Act
            var result = await service.GetAll();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, ((List<PatientDto>)result.Data!).Count);
        }

        [Fact]
        public async Task GetByAppUserId_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            var patient = new Patient { Id = "p-1", SocialSecurityNumber = "001-1", Address = "Calle 1", AppUserId = "user-5" };
            patientRepo.Setup(r => r.GetByAppUserId("user-5")).ReturnsAsync(new OperationResult { Data = patient });

            // Act
            var result = await service.GetByAppUserId("user-5");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("user-5", ((PatientDto)result.Data!).AppUserId);
        }

        [Fact]
        public async Task GetByAppUserId_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetByAppUserId("user-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Paciente no encontrado." });

            // Act
            var result = await service.GetByAppUserId("user-x");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Paciente no encontrado.", result.Message);
        }

        [Fact]
        public async Task GetBySocialSecurityNumber_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            var patient = new Patient { Id = "p-1", SocialSecurityNumber = "001-1234567-8", Address = "Calle 1", AppUserId = "user-1" };
            patientRepo.Setup(r => r.GetBySocialSecurityNumberAsync("001-1234567-8")).ReturnsAsync(new OperationResult { Data = patient });

            // Act
            var result = await service.GetBySocialSecurityNumber("001-1234567-8");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("001-1234567-8", ((PatientDto)result.Data!).SocialSecurityNumber);
        }

        [Fact]
        public async Task GetBySocialSecurityNumber_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (patientRepo, service) = CreateService();
            patientRepo.Setup(r => r.GetBySocialSecurityNumberAsync("000-0000000-0"))
                .ReturnsAsync(new OperationResult { Success = false, Message = "No existe un paciente con ese número de seguro social." });

            // Act
            var result = await service.GetBySocialSecurityNumber("000-0000000-0");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No existe un paciente con ese número de seguro social.", result.Message);
        }
    }
}
