namespace SGCM.Application.DTOs.Doctor
{
    public class UpdateDoctorDto
    {
        public string Id { get; set; } = string.Empty;
        public string MedicalLicense { get; set; } = string.Empty;
        public string SpecialtyId { get; set; } = string.Empty;
    }
}
