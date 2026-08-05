using System;

namespace SGCM.Application.DTOs.Patient
{
    public class UpdatePatientDto
    {
        public string Id { get; set; } = string.Empty;
        public string SocialSecurityNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}
