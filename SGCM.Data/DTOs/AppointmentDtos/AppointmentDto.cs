using SGCM.Domain.Enums;

namespace SGCM.Data.DTOs.AppointmentDtos
{
    public class AppointmentDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
    }
}
