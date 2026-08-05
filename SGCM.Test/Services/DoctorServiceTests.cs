using Moq;
using SGCM.Application.DTOs.Doctor;
using SGCM.Application.Services;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;
using System.Linq.Expressions;
using Xunit;

namespace SGCM.Test.Services
{
    public class DoctorServiceTests
    {
        private static (Mock<IDoctorRepository> doctorRepo, Mock<ISpecialtyRepository> specialtyRepo, DoctorService service) CreateService()
        {
            var doctorRepo = new Mock<IDoctorRepository>();
            var specialtyRepo = new Mock<ISpecialtyRepository>();
            var service = new DoctorService(doctorRepo.Object, specialtyRepo.Object);
            return (doctorRepo, specialtyRepo, service);
        }

        [Fact]
        public async Task Create_ValidDto_ShouldReturnSuccessWithDto()
        {
            // Arrange
            var (doctorRepo, specialtyRepo, service) = CreateService();
            var dto = new CreateDoctorDto { MedicalLicense = "LIC-001", AppUserId = "user-1", SpecialtyId = "spec-1" };

            doctorRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Doctor, bool>>>())).ReturnsAsync(false);
            specialtyRepo.Setup(r => r.GetById("spec-1")).ReturnsAsync(new OperationResult { Data = new Specialty { Id = "spec-1" } });
            doctorRepo.Setup(r => r.Add(It.IsAny<Doctor>())).ReturnsAsync((Doctor d) => new OperationResult { Data = d });

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.True(result.Success);
            var returned = (DoctorDto)result.Data!;
            Assert.Equal("LIC-001", returned.MedicalLicense);
            Assert.Equal("spec-1", returned.SpecialtyId);
        }

        [Fact]
        public async Task Create_EmptyMedicalLicense_ShouldFail()
        {
            // Arrange
            var (_, _, service) = CreateService();
            var dto = new CreateDoctorDto { MedicalLicense = "", AppUserId = "user-1", SpecialtyId = "spec-1" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("La licencia médica no puede estar vacía.", result.Message);
        }

        [Fact]
        public async Task Create_DuplicateMedicalLicense_ShouldFail()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            doctorRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Doctor, bool>>>())).ReturnsAsync(true);
            var dto = new CreateDoctorDto { MedicalLicense = "LIC-001", AppUserId = "user-1", SpecialtyId = "spec-1" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Ya existe un doctor con esa licencia médica.", result.Message);
        }

        [Fact]
        public async Task Create_SpecialtyDoesNotExist_ShouldFail()
        {
            // Arrange
            var (doctorRepo, specialtyRepo, service) = CreateService();
            doctorRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Doctor, bool>>>())).ReturnsAsync(false);
            specialtyRepo.Setup(r => r.GetById("spec-inexistente")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new CreateDoctorDto { MedicalLicense = "LIC-001", AppUserId = "user-1", SpecialtyId = "spec-inexistente" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("La especialidad especificada no existe.", result.Message);
        }

        [Fact]
        public async Task GetById_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            var doctor = new Doctor { Id = "doc-1", MedicalLicense = "LIC-001", AppUserId = "user-1", SpecialtyId = "spec-1" };
            doctorRepo.Setup(r => r.GetById("doc-1")).ReturnsAsync(new OperationResult { Data = doctor });

            // Act
            var result = await service.GetById("doc-1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("doc-1", ((DoctorDto)result.Data!).Id);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ShouldPropagateFailure()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            doctorRepo.Setup(r => r.GetById("doc-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });

            // Act
            var result = await service.GetById("doc-x");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Registro no encontrado.", result.Message);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedList()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            var doctors = new List<Doctor>
            {
                new Doctor { Id = "doc-1", MedicalLicense = "LIC-001", AppUserId = "user-1", SpecialtyId = "spec-1" },
                new Doctor { Id = "doc-2", MedicalLicense = "LIC-002", AppUserId = "user-2", SpecialtyId = "spec-1" }
            };
            doctorRepo.Setup(r => r.GetAll()).ReturnsAsync(new OperationResult { Data = doctors });

            // Act
            var result = await service.GetAll();

            // Assert
            Assert.True(result.Success);
            var returned = (List<DoctorDto>)result.Data!;
            Assert.Equal(2, returned.Count);
        }

        [Fact]
        public async Task Update_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            doctorRepo.Setup(r => r.GetById("doc-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new UpdateDoctorDto { Id = "doc-x", MedicalLicense = "LIC-001", SpecialtyId = "spec-1" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Registro no encontrado.", result.Message);
        }

        [Fact]
        public async Task Update_DuplicateLicenseOnAnotherDoctor_ShouldFail()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            var doctor = new Doctor { Id = "doc-1", MedicalLicense = "LIC-001", AppUserId = "user-1", SpecialtyId = "spec-1" };
            doctorRepo.Setup(r => r.GetById("doc-1")).ReturnsAsync(new OperationResult { Data = doctor });
            doctorRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Doctor, bool>>>())).ReturnsAsync(true);
            var dto = new UpdateDoctorDto { Id = "doc-1", MedicalLicense = "LIC-999", SpecialtyId = "spec-1" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Ya existe otro doctor con esa licencia médica.", result.Message);
        }

        [Fact]
        public async Task Update_Valid_ShouldReturnUpdatedDto()
        {
            // Arrange
            var (doctorRepo, specialtyRepo, service) = CreateService();
            var doctor = new Doctor { Id = "doc-1", MedicalLicense = "LIC-001", AppUserId = "user-1", SpecialtyId = "spec-1" };
            doctorRepo.Setup(r => r.GetById("doc-1")).ReturnsAsync(new OperationResult { Data = doctor });
            doctorRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Doctor, bool>>>())).ReturnsAsync(false);
            specialtyRepo.Setup(r => r.GetById("spec-2")).ReturnsAsync(new OperationResult { Data = new Specialty { Id = "spec-2" } });
            doctorRepo.Setup(r => r.Update(It.IsAny<Doctor>())).ReturnsAsync((Doctor d) => new OperationResult { Data = d });
            var dto = new UpdateDoctorDto { Id = "doc-1", MedicalLicense = "LIC-002", SpecialtyId = "spec-2" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.True(result.Success);
            var returned = (DoctorDto)result.Data!;
            Assert.Equal("LIC-002", returned.MedicalLicense);
            Assert.Equal("spec-2", returned.SpecialtyId);
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
            Assert.Equal("El identificador del doctor no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task Delete_ShouldDelegateToRepository()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            doctorRepo.Setup(r => r.Delete("doc-1")).ReturnsAsync(new OperationResult { Message = "Registro eliminado con exito." });

            // Act
            var result = await service.Delete("doc-1");

            // Assert
            Assert.True(result.Success);
            doctorRepo.Verify(r => r.Delete("doc-1"), Times.Once);
        }

        [Fact]
        public async Task GetByAppUserId_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            var doctor = new Doctor { Id = "doc-1", MedicalLicense = "LIC-001", AppUserId = "user-7", SpecialtyId = "spec-1" };
            doctorRepo.Setup(r => r.GetByAppUserId("user-7")).ReturnsAsync(new OperationResult { Data = doctor });

            // Act
            var result = await service.GetByAppUserId("user-7");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("user-7", ((DoctorDto)result.Data!).AppUserId);
        }

        [Fact]
        public async Task GetByAppUserId_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            doctorRepo.Setup(r => r.GetByAppUserId("user-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Doctor no encontrado." });

            // Act
            var result = await service.GetByAppUserId("user-x");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Doctor no encontrado.", result.Message);
        }

        [Fact]
        public async Task GetBySpecialty_ShouldReturnMappedList()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            var doctors = new List<Doctor> { new Doctor { Id = "doc-1", MedicalLicense = "LIC-001", AppUserId = "user-1", SpecialtyId = "cardio" } };
            doctorRepo.Setup(r => r.GetBySpecialty("cardio")).ReturnsAsync(new OperationResult { Data = doctors });

            // Act
            var result = await service.GetBySpecialty("cardio");

            // Assert
            Assert.True(result.Success);
            var returned = (List<DoctorDto>)result.Data!;
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetByMedicalLicense_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            var doctor = new Doctor { Id = "doc-1", MedicalLicense = "LIC-123", AppUserId = "user-1", SpecialtyId = "spec-1" };
            doctorRepo.Setup(r => r.GetByMedicalLicenseAsync("LIC-123")).ReturnsAsync(new OperationResult { Data = doctor });

            // Act
            var result = await service.GetByMedicalLicense("LIC-123");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("LIC-123", ((DoctorDto)result.Data!).MedicalLicense);
        }

        [Fact]
        public async Task GetByMedicalLicense_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (doctorRepo, _, service) = CreateService();
            doctorRepo.Setup(r => r.GetByMedicalLicenseAsync("LIC-999")).ReturnsAsync(new OperationResult { Success = false, Message = "No existe un doctor con esa licencia." });

            // Act
            var result = await service.GetByMedicalLicense("LIC-999");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No existe un doctor con esa licencia.", result.Message);
        }
    }
}
