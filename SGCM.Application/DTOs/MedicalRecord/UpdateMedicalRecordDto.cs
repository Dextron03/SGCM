namespace SGCM.Application.DTOs.MedicalRecord
{
    public class UpdateMedicalRecordDto
    {
        public string Id { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
