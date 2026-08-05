using SGCM.Application.DTOs.Patient;
using SGCM.Application.Interfaces;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;

namespace SGCM.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<OperationResult> GetAll()
        {
            var result = await _patientRepository.GetAll();
            if (!result.Success) return result;
            var patients = (List<Patient>)result.Data!;
            result.Data = patients.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetById(string id)
        {
            var result = await _patientRepository.GetById(id);
            if (!result.Success) return result;
            result.Data = ToDto((Patient)result.Data!);
            return result;
        }

        public async Task<OperationResult> Create(CreatePatientDto dto)
        {
            if (dto is null)
                return new OperationResult { Success = false, Message = "Los datos del paciente no pueden estar vacíos." };
            if (string.IsNullOrWhiteSpace(dto.SocialSecurityNumber))
                return new OperationResult { Success = false, Message = "El número de seguro social no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Address))
                return new OperationResult { Success = false, Message = "La dirección no puede estar vacía." };
            if (string.IsNullOrWhiteSpace(dto.AppUserId))
                return new OperationResult { Success = false, Message = "El usuario asociado no puede estar vacío." };
            if (dto.DateOfBirth == default)
                return new OperationResult { Success = false, Message = "La fecha de nacimiento no es válida." };

            var ssnExists = await _patientRepository.Exists(p => p.SocialSecurityNumber == dto.SocialSecurityNumber);
            if (ssnExists)
                return new OperationResult { Success = false, Message = "Ya existe un paciente con ese número de seguro social." };

            var patient = new Patient
            {
                SocialSecurityNumber = dto.SocialSecurityNumber,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                AppUserId = dto.AppUserId
            };

            var result = await _patientRepository.Add(patient);
            if (!result.Success) return result;
            result.Data = ToDto((Patient)result.Data!);
            return result;
        }

        public async Task<OperationResult> Update(UpdatePatientDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
                return new OperationResult { Success = false, Message = "El identificador del paciente no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.SocialSecurityNumber))
                return new OperationResult { Success = false, Message = "El número de seguro social no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Address))
                return new OperationResult { Success = false, Message = "La dirección no puede estar vacía." };

            var existingResult = await _patientRepository.GetById(dto.Id);
            if (!existingResult.Success) return existingResult;
            var patient = (Patient)existingResult.Data!;

            if (patient.SocialSecurityNumber != dto.SocialSecurityNumber)
            {
                var duplicated = await _patientRepository.Exists(p => p.SocialSecurityNumber == dto.SocialSecurityNumber && p.Id != dto.Id);
                if (duplicated)
                    return new OperationResult { Success = false, Message = "Ya existe otro paciente con ese número de seguro social." };
            }

            patient.SocialSecurityNumber = dto.SocialSecurityNumber;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.Address = dto.Address;

            var result = await _patientRepository.Update(patient);
            if (!result.Success) return result;
            result.Data = ToDto((Patient)result.Data!);
            return result;
        }

        public async Task<OperationResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new OperationResult { Success = false, Message = "El identificador del paciente no puede estar vacío." };
            return await _patientRepository.Delete(id);
        }

        public async Task<OperationResult> GetByAppUserId(string appUserId)
        {
            var result = await _patientRepository.GetByAppUserId(appUserId);
            if (!result.Success) return result;
            result.Data = ToDto((Patient)result.Data!);
            return result;
        }

        public async Task<OperationResult> GetBySocialSecurityNumber(string socialSecurityNumber)
        {
            var result = await _patientRepository.GetBySocialSecurityNumberAsync(socialSecurityNumber);
            if (!result.Success) return result;
            result.Data = ToDto((Patient)result.Data!);
            return result;
        }

        private static PatientDto ToDto(Patient patient) => new PatientDto
        {
            Id = patient.Id,
            SocialSecurityNumber = patient.SocialSecurityNumber,
            DateOfBirth = patient.DateOfBirth,
            Address = patient.Address,
            AppUserId = patient.AppUserId
        };
    }
}
