using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGCM.Domain.Enums;

namespace SGCM.Domain.Entities
{
    public class Doctor
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MedicalLicense { get; set; } = string.Empty; 

        // Login Relationship
        public string AppUserId { get; set; } = string.Empty;
        public AppUser AppUser { get; set; } = null!;

        // Specialty Relationship
        public string SpecialtyId { get; set; } = string.Empty;
        public Specialty Specialty { get; set; } = null!;

        // Operational Relationships
        public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
