using System;

namespace SGCM.Application.DTOs.Appointment
{
    public class CreateAppointmentDto
    {
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
