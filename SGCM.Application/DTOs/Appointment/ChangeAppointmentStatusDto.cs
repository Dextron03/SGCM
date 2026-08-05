using SGCM.Domain.Enums;

namespace SGCM.Application.DTOs.Appointment
{
    public class ChangeAppointmentStatusDto
    {
        public string Id { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }
    }
}
