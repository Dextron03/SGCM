using Moq;
using SGCM.Application.DTOs.Specialty;
using SGCM.Application.Services;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;
using System.Linq.Expressions;
using Xunit;

namespace SGCM.Test.Services
{
    public class SpecialtyServiceTests
    {
        private static (Mock<ISpecialtyRepository> specialtyRepo, SpecialtyService service) CreateService()
        {
            var specialtyRepo = new Mock<ISpecialtyRepository>();
            var service = new SpecialtyService(specialtyRepo.Object);
            return (specialtyRepo, service);
        }

        [Fact]
        public async Task Create_ValidDto_ShouldReturnSuccessWithDto()
        {
            // Arrange
            var (specialtyRepo, service) = CreateService();
            specialtyRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Specialty, bool>>>())).ReturnsAsync(false);
            specialtyRepo.Setup(r => r.Add(It.IsAny<Specialty>())).ReturnsAsync((Specialty s) => new OperationResult { Data = s });
            var dto = new CreateSpecialtyDto { Name = "Cardiología", Description = "Corazón" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Cardiología", ((SpecialtyDto)result.Data!).Name);
        }

        [Fact]
        public async Task Create_EmptyName_ShouldFail()
        {
            // Arrange
            var (_, service) = CreateService();
            var dto = new CreateSpecialtyDto { Name = "", Description = "Corazón" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("El nombre de la especialidad no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task Create_DuplicateName_ShouldFail()
        {
            // Arrange
            var (specialtyRepo, service) = CreateService();
            specialtyRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Specialty, bool>>>())).ReturnsAsync(true);
            var dto = new CreateSpecialtyDto { Name = "Cardiología", Description = "Corazón" };

            // Act
            var result = await service.Create(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Ya existe una especialidad con ese nombre.", result.Message);
        }

        [Fact]
        public async Task Update_WhenNotFound_ShouldFail()
        {
            // Arrange
            var (specialtyRepo, service) = CreateService();
            specialtyRepo.Setup(r => r.GetById("s-x")).ReturnsAsync(new OperationResult { Success = false, Message = "Registro no encontrado." });
            var dto = new UpdateSpecialtyDto { Id = "s-x", Name = "Cardiología", Description = "Corazón" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Registro no encontrado.", result.Message);
        }

        [Fact]
        public async Task Update_DuplicateNameOnAnotherSpecialty_ShouldFail()
        {
            // Arrange
            var (specialtyRepo, service) = CreateService();
            var specialty = new Specialty { Id = "s-1", Name = "Cardiología", Description = "Corazón" };
            specialtyRepo.Setup(r => r.GetById("s-1")).ReturnsAsync(new OperationResult { Data = specialty });
            specialtyRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Specialty, bool>>>())).ReturnsAsync(true);
            var dto = new UpdateSpecialtyDto { Id = "s-1", Name = "Pediatría", Description = "Niños" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Ya existe otra especialidad con ese nombre.", result.Message);
        }

        [Fact]
        public async Task Update_Valid_ShouldReturnUpdatedDto()
        {
            // Arrange
            var (specialtyRepo, service) = CreateService();
            var specialty = new Specialty { Id = "s-1", Name = "Cardiología", Description = "Corazón" };
            specialtyRepo.Setup(r => r.GetById("s-1")).ReturnsAsync(new OperationResult { Data = specialty });
            specialtyRepo.Setup(r => r.Exists(It.IsAny<Expression<Func<Specialty, bool>>>())).ReturnsAsync(false);
            specialtyRepo.Setup(r => r.Update(It.IsAny<Specialty>())).ReturnsAsync((Specialty s) => new OperationResult { Data = s });
            var dto = new UpdateSpecialtyDto { Id = "s-1", Name = "Cardiología Pediátrica", Description = "Corazón de niños" };

            // Act
            var result = await service.Update(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Cardiología Pediátrica", ((SpecialtyDto)result.Data!).Name);
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
            Assert.Equal("El identificador de la especialidad no puede estar vacío.", result.Message);
        }

        [Fact]
        public async Task GetById_WhenFound_ShouldReturnDto()
        {
            // Arrange
            var (specialtyRepo, service) = CreateService();
            var specialty = new Specialty { Id = "s-1", Name = "Cardiología", Description = "Corazón" };
            specialtyRepo.Setup(r => r.GetById("s-1")).ReturnsAsync(new OperationResult { Data = specialty });

            // Act
            var result = await service.GetById("s-1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("s-1", ((SpecialtyDto)result.Data!).Id);
        }

        [Fact]
        public async Task GetAll_ShouldReturnMappedList()
        {
            // Arrange
            var (specialtyRepo, service) = CreateService();
            var specialties = new List<Specialty>
            {
                new Specialty { Id = "s-1", Name = "Cardiología", Description = "Corazón" },
                new Specialty { Id = "s-2", Name = "Pediatría", Description = "Niños" }
            };
            specialtyRepo.Setup(r => r.GetAll()).ReturnsAsync(new OperationResult { Data = specialties });

            // Act
            var result = await service.GetAll();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, ((List<SpecialtyDto>)result.Data!).Count);
        }
    }
}
