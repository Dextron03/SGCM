using System;

namespace SGCM.Application.DTOs.Patient
{
    public class CreatePatientDto
    {
        public string SocialSecurityNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string AppUserId { get; set; } = string.Empty;
    }
}
