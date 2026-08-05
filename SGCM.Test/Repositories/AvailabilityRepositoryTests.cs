using SGCM.Data.Repositories;
using SGCM.Domain.Entities;
using SGCM.Domain.Enums;
using SGCM.Test.Common;
using Xunit;

namespace SGCM.Test.Repositories
{
    public class AvailabilityRepositoryTests
    {
        private static Availability NewAvailability(
            string doctorId = "doctor-1",
            AvailableDay day = AvailableDay.Monday) => new Availability
        {
            DoctorId = doctorId,
            Day = day,
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(12, 0, 0)
        };

        [Fact]
        public async Task GetByDoctor_ShouldReturnOnlyDoctorAvailabilities()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new AvailabilityRepository(context);
            await repository.Add(NewAvailability(doctorId: "doctor-1", day: AvailableDay.Monday));
            await repository.Add(NewAvailability(doctorId: "doctor-1", day: AvailableDay.Wednesday));
            await repository.Add(NewAvailability(doctorId: "doctor-2", day: AvailableDay.Friday));

            var result = await repository.GetByDoctor("doctor-1");

            Assert.True(result.Success);
            var availabilities = (List<Availability>)result.Data!;
            Assert.Equal(2, availabilities.Count);
            Assert.All(availabilities, a => Assert.Equal("doctor-1", a.DoctorId));
        }

        [Fact]
        public async Task GetByDoctor_WhenDoctorHasNoAvailability_ShouldReturnEmptyList()
        {
            using var context = TestDbContextFactory.Create();
            var repository = new AvailabilityRepository(context);
            await repository.Add(NewAvailability(doctorId: "doctor-1"));

            var result = await repository.GetByDoctor("doctor-sin-horario");

            Assert.True(result.Success);
            var availabilities = (List<Availability>)result.Data!;
            Assert.Empty(availabilities);
        }
    }
}
