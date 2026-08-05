using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGCM.Application.DTOs.Patient
{
    public class PatientDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SocialSecurityNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;

        public string AppUserId { get; set; } = string.Empty;

    }
}
