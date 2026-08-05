using SGCM.Application.DTOs.MedicalRecord;
using SGCM.Application.Interfaces;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;

namespace SGCM.Application.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public MedicalRecordService(
            IMedicalRecordRepository medicalRecordRepository,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository,
            IAppointmentRepository appointmentRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<OperationResult> GetAll()
        {
            var result = await _medicalRecordRepository.GetAll();
            if (!result.Success) return result;
            var records = (List<MedicalRecord>)result.Data!;
            result.Data = records.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetById(string id)
        {
            var result = await _medicalRecordRepository.GetById(id);
            if (!result.Success) return result;
            result.Data = ToDto((MedicalRecord)result.Data!);
            return result;
        }

        public async Task<OperationResult> Create(CreateMedicalRecordDto dto)
        {
            if (dto is null)
                return new OperationResult { Success = false, Message = "Los datos del registro médico no pueden estar vacíos." };
            if (string.IsNullOrWhiteSpace(dto.Diagnosis))
                return new OperationResult { Success = false, Message = "El diagnóstico no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Treatment))
                return new OperationResult { Success = false, Message = "El tratamiento no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.PatientId))
                return new OperationResult { Success = false, Message = "El paciente no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.DoctorId))
                return new OperationResult { Success = false, Message = "El doctor no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.AppointmentId))
                return new OperationResult { Success = false, Message = "La cita no puede estar vacía." };

            var patientResult = await _patientRepository.GetById(dto.PatientId);
            if (!patientResult.Success)
                return new OperationResult { Success = false, Message = "El paciente especificado no existe." };

            var doctorResult = await _doctorRepository.GetById(dto.DoctorId);
            if (!doctorResult.Success)
                return new OperationResult { Success = false, Message = "El doctor especificado no existe." };

            var appointmentResult = await _appointmentRepository.GetById(dto.AppointmentId);
            if (!appointmentResult.Success)
                return new OperationResult { Success = false, Message = "La cita especificada no existe." };

            var existingRecordResult = await _medicalRecordRepository.GetByAppointment(dto.AppointmentId);
            if (existingRecordResult.Success)
                return new OperationResult { Success = false, Message = "La cita ya tiene un registro médico asociado." };

            var medicalRecord = new MedicalRecord
            {
                Diagnosis = dto.Diagnosis,
                Treatment = dto.Treatment,
                Notes = dto.Notes,
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                AppointmentId = dto.AppointmentId
            };

            var result = await _medicalRecordRepository.Add(medicalRecord);
            if (!result.Success) return result;
            result.Data = ToDto((MedicalRecord)result.Data!);
            return result;
        }

        public async Task<OperationResult> Update(UpdateMedicalRecordDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
                return new OperationResult { Success = false, Message = "El identificador del registro médico no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Diagnosis))
                return new OperationResult { Success = false, Message = "El diagnóstico no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Treatment))
                return new OperationResult { Success = false, Message = "El tratamiento no puede estar vacío." };

            var existingResult = await _medicalRecordRepository.GetById(dto.Id);
            if (!existingResult.Success) return existingResult;
            var medicalRecord = (MedicalRecord)existingResult.Data!;

            medicalRecord.Diagnosis = dto.Diagnosis;
            medicalRecord.Treatment = dto.Treatment;
            medicalRecord.Notes = dto.Notes;

            var result = await _medicalRecordRepository.Update(medicalRecord);
            if (!result.Success) return result;
            result.Data = ToDto((MedicalRecord)result.Data!);
            return result;
        }

        public async Task<OperationResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new OperationResult { Success = false, Message = "El identificador del registro médico no puede estar vacío." };
            return await _medicalRecordRepository.Delete(id);
        }

        public async Task<OperationResult> GetByPatient(string patientId)
        {
            var result = await _medicalRecordRepository.GetByPatient(patientId);
            if (!result.Success) return result;
            var records = (List<MedicalRecord>)result.Data!;
            result.Data = records.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetByAppointment(string appointmentId)
        {
            var result = await _medicalRecordRepository.GetByAppointment(appointmentId);
            if (!result.Success) return result;
            result.Data = ToDto((MedicalRecord)result.Data!);
            return result;
        }

        private static MedicalRecordDto ToDto(MedicalRecord medicalRecord) => new MedicalRecordDto
        {
            Id = medicalRecord.Id,
            Diagnosis = medicalRecord.Diagnosis,
            Treatment = medicalRecord.Treatment,
            Notes = medicalRecord.Notes,
            CreationDate = medicalRecord.CreationDate,
            PatientId = medicalRecord.PatientId,
            DoctorId = medicalRecord.DoctorId,
            AppointmentId = medicalRecord.AppointmentId
        };
    }
}
