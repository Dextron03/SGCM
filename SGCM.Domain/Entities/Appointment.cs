using SGCM.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGCM.Domain.Entities
{
    public class Appointment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime DateTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        // Foreign Keys
        public string PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public MedicalRecord? MedicalRecord { get; set; }
    }
}
