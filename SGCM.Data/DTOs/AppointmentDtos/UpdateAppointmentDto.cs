using SGCM.Domain.Enums;

namespace SGCM.Data.DTOs.AppointmentDtos
{
    public class UpdateAppointmentDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime? DateTime { get; set; }
        public string? Reason { get; set; }
        public AppointmentStatus? Status { get; set; }
    }
}
