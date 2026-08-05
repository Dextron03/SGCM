using System;
using SGCM.Domain.Enums;

namespace SGCM.Application.DTOs.Availability
{
    public class UpdateAvailabilityDto
    {
        public string Id { get; set; } = string.Empty;
        public AvailableDay Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
