using SGCM.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGCM.Application.DTOs.Availability
{
    public class AvailabilityDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string DoctorId { get; set; } = string.Empty;

        public AvailableDay Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

    }
}
