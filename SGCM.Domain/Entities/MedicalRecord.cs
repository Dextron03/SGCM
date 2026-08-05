using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGCM.Domain.Entities
{
    public class MedicalRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public string PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        public string DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public string AppointmentId { get; set; }
        public Appointment Appointment { get; set; } = null!;
    }
}
