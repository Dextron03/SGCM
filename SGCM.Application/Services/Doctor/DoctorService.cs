using SGCM.Application.DTOs.Doctor;
using SGCM.Application.Interfaces;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;

namespace SGCM.Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ISpecialtyRepository _specialtyRepository;

        public DoctorService(IDoctorRepository doctorRepository, ISpecialtyRepository specialtyRepository)
        {
            _doctorRepository = doctorRepository;
            _specialtyRepository = specialtyRepository;
        }

        public async Task<OperationResult> GetAll()
        {
            var result = await _doctorRepository.GetAll();
            if (!result.Success) return result;
            var doctors = (List<Doctor>)result.Data!;
            result.Data = doctors.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetById(string id)
        {
            var result = await _doctorRepository.GetById(id);
            if (!result.Success) return result;
            result.Data = ToDto((Doctor)result.Data!);
            return result;
        }

        public async Task<OperationResult> Create(CreateDoctorDto dto)
        {
            if (dto is null)
                return new OperationResult { Success = false, Message = "Los datos del doctor no pueden estar vacíos." };
            if (string.IsNullOrWhiteSpace(dto.MedicalLicense))
                return new OperationResult { Success = false, Message = "La licencia médica no puede estar vacía." };
            if (string.IsNullOrWhiteSpace(dto.AppUserId))
                return new OperationResult { Success = false, Message = "El usuario asociado no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.SpecialtyId))
                return new OperationResult { Success = false, Message = "La especialidad no puede estar vacía." };

            var licenseExists = await _doctorRepository.Exists(d => d.MedicalLicense == dto.MedicalLicense);
            if (licenseExists)
                return new OperationResult { Success = false, Message = "Ya existe un doctor con esa licencia médica." };

            var specialtyResult = await _specialtyRepository.GetById(dto.SpecialtyId);
            if (!specialtyResult.Success)
                return new OperationResult { Success = false, Message = "La especialidad especificada no existe." };

            var doctor = new Doctor
            {
                MedicalLicense = dto.MedicalLicense,
                AppUserId = dto.AppUserId,
                SpecialtyId = dto.SpecialtyId
            };

            var result = await _doctorRepository.Add(doctor);
            if (!result.Success) return result;
            result.Data = ToDto((Doctor)result.Data!);
            return result;
        }

        public async Task<OperationResult> Update(UpdateDoctorDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
                return new OperationResult { Success = false, Message = "El identificador del doctor no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.MedicalLicense))
                return new OperationResult { Success = false, Message = "La licencia médica no puede estar vacía." };
            if (string.IsNullOrWhiteSpace(dto.SpecialtyId))
                return new OperationResult { Success = false, Message = "La especialidad no puede estar vacía." };

            var existingResult = await _doctorRepository.GetById(dto.Id);
            if (!existingResult.Success) return existingResult;
            var doctor = (Doctor)existingResult.Data!;

            if (doctor.MedicalLicense != dto.MedicalLicense)
            {
                var duplicated = await _doctorRepository.Exists(d => d.MedicalLicense == dto.MedicalLicense && d.Id != dto.Id);
                if (duplicated)
                    return new OperationResult { Success = false, Message = "Ya existe otro doctor con esa licencia médica." };
            }

            if (doctor.SpecialtyId != dto.SpecialtyId)
            {
                var specialtyResult = await _specialtyRepository.GetById(dto.SpecialtyId);
                if (!specialtyResult.Success)
                    return new OperationResult { Success = false, Message = "La especialidad especificada no existe." };
            }

            doctor.MedicalLicense = dto.MedicalLicense;
            doctor.SpecialtyId = dto.SpecialtyId;

            var result = await _doctorRepository.Update(doctor);
            if (!result.Success) return result;
            result.Data = ToDto((Doctor)result.Data!);
            return result;
        }

        public async Task<OperationResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new OperationResult { Success = false, Message = "El identificador del doctor no puede estar vacío." };
            return await _doctorRepository.Delete(id);
        }

        public async Task<OperationResult> GetByAppUserId(string appUserId)
        {
            var result = await _doctorRepository.GetByAppUserId(appUserId);
            if (!result.Success) return result;
            result.Data = ToDto((Doctor)result.Data!);
            return result;
        }

        public async Task<OperationResult> GetBySpecialty(string specialtyId)
        {
            var result = await _doctorRepository.GetBySpecialty(specialtyId);
            if (!result.Success) return result;
            var doctors = (List<Doctor>)result.Data!;
            result.Data = doctors.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetByMedicalLicense(string medicalLicense)
        {
            var result = await _doctorRepository.GetByMedicalLicenseAsync(medicalLicense);
            if (!result.Success) return result;
            result.Data = ToDto((Doctor)result.Data!);
            return result;
        }

        private static DoctorDto ToDto(Doctor doctor) => new DoctorDto
        {
            Id = doctor.Id,
            MedicalLicense = doctor.MedicalLicense,
            AppUserId = doctor.AppUserId,
            SpecialtyId = doctor.SpecialtyId
        };
    }
}
