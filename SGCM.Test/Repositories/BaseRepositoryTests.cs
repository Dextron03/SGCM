using SGCM.Data.Repositories;
using SGCM.Domain.Entities;
using SGCM.Test.Common;
using Xunit;

namespace SGCM.Test.Repositories
{
    /// <summary>
    /// Prueba el CRUD generico de BaseRepository usando Specialty como entidad.
    /// </summary>
    public class BaseRepositoryTests
    {
        private static Specialty NewSpecialty(string name = "Cardiologia") => new Specialty
        {
            Name = name,
            Description = $"Especialidad de {name}"
        };

        [Fact]
        public async Task Add_ShouldPersistEntity()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new BaseRepository<Specialty>(context);

            var result = await repository.Add(NewSpecialty());

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, context.Specialties.Count());
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllRecords()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new BaseRepository<Specialty>(context);
            await repository.Add(NewSpecialty("Cardiologia"));
            await repository.Add(NewSpecialty("Pediatria"));

            var result = await repository.GetAll();

            Assert.True(result.Success);
            var specialties = (List<Specialty>)result.Data!;
            Assert.Equal(2, specialties.Count);
        }

        [Fact]
        public async Task GetById_WhenEntityExists_ShouldReturnIt()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new BaseRepository<Specialty>(context);
            var specialty = NewSpecialty();
            await repository.Add(specialty);

            var result = await repository.GetById(specialty.Id);

            Assert.True(result.Success);
            var found = (Specialty)result.Data!;
            Assert.Equal(specialty.Id, found.Id);
        }

        [Fact]
        public async Task GetById_WhenEntityDoesNotExist_ShouldFail()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new BaseRepository<Specialty>(context);

            var result = await repository.GetById("id-inexistente");

            Assert.False(result.Success);
            Assert.Equal("Registro no encontrado.", result.Message);
        }

        [Fact]
        public async Task Update_ShouldModifyEntity()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new BaseRepository<Specialty>(context);
            var specialty = NewSpecialty();
            await repository.Add(specialty);

            specialty.Name = "Dermatologia";
            var result = await repository.Update(specialty);

            Assert.True(result.Success);
            var updated = context.Specialties.Single(s => s.Id == specialty.Id);
            Assert.Equal("Dermatologia", updated.Name);
        }

        [Fact]
        public async Task Delete_WhenEntityExists_ShouldRemoveIt()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new BaseRepository<Specialty>(context);
            var specialty = NewSpecialty();
            await repository.Add(specialty);

            var result = await repository.Delete(specialty.Id);

            Assert.True(result.Success);
            Assert.Equal(0, context.Specialties.Count());
        }

        [Fact]
        public async Task Delete_WhenEntityDoesNotExist_ShouldFail()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new BaseRepository<Specialty>(context);

            var result = await repository.Delete("id-inexistente");

            Assert.False(result.Success);
            Assert.Equal("Registro no encontrado.", result.Message);
        }

        [Fact]
        public async Task Exists_WhenPredicateMatches_ShouldReturnTrue()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new BaseRepository<Specialty>(context);
            await repository.Add(NewSpecialty("Cardiologia"));

            var exists = await repository.Exists(s => s.Name == "Cardiologia");

            Assert.True(exists);
        }

        [Fact]
        public async Task Exists_WhenPredicateDoesNotMatch_ShouldReturnFalse()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new BaseRepository<Specialty>(context);
            await repository.Add(NewSpecialty("Cardiologia"));

            var exists = await repository.Exists(s => s.Name == "Neurologia");

            Assert.False(exists);
        }
    }
}
