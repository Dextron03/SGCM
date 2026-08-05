using SGCM.Domain.Enums;

namespace SGCM.Data.DTOs.AvailabilityDtos
{
    public class CreateAvailabilityDto
    {
        public string DoctorId { get; set; } = string.Empty;
        public AvailableDay Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
