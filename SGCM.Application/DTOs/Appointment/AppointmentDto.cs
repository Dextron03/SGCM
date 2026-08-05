using SGCM.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGCM.Application.DTOs.Appointment
{
    public class AppointmentDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime DateTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
    }
}
