using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGCM.Application.DTOs.Doctor
{
    public class DoctorDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MedicalLicense { get; set; } = string.Empty;
        public string AppUserId { get; set; } = string.Empty;
        public string SpecialtyId { get; set; } = string.Empty;

    }
}
