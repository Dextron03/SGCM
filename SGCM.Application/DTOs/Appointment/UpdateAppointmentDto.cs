using System;

namespace SGCM.Application.DTOs.Appointment
{
    public class UpdateAppointmentDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
