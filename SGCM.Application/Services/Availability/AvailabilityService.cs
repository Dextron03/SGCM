using SGCM.Application.DTOs.Availability;
using SGCM.Application.Interfaces;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;

namespace SGCM.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IDoctorRepository _doctorRepository;

        public AvailabilityService(IAvailabilityRepository availabilityRepository, IDoctorRepository doctorRepository)
        {
            _availabilityRepository = availabilityRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<OperationResult> GetAll()
        {
            var result = await _availabilityRepository.GetAll();
            if (!result.Success) return result;
            var availabilities = (List<Availability>)result.Data!;
            result.Data = availabilities.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetById(string id)
        {
            var result = await _availabilityRepository.GetById(id);
            if (!result.Success) return result;
            result.Data = ToDto((Availability)result.Data!);
            return result;
        }

        public async Task<OperationResult> Create(CreateAvailabilityDto dto)
        {
            if (dto is null)
                return new OperationResult { Success = false, Message = "Los datos de la disponibilidad no pueden estar vacíos." };
            if (string.IsNullOrWhiteSpace(dto.DoctorId))
                return new OperationResult { Success = false, Message = "El doctor no puede estar vacío." };
            if (dto.StartTime >= dto.EndTime)
                return new OperationResult { Success = false, Message = "La hora de inicio debe ser anterior a la hora de fin." };

            var doctorResult = await _doctorRepository.GetById(dto.DoctorId);
            if (!doctorResult.Success)
                return new OperationResult { Success = false, Message = "El doctor especificado no existe." };

            var availability = new Availability
            {
                DoctorId = dto.DoctorId,
                Day = dto.Day,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };

            var result = await _availabilityRepository.Add(availability);
            if (!result.Success) return result;
            result.Data = ToDto((Availability)result.Data!);
            return result;
        }

        public async Task<OperationResult> Update(UpdateAvailabilityDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
                return new OperationResult { Success = false, Message = "El identificador de la disponibilidad no puede estar vacío." };
            if (dto.StartTime >= dto.EndTime)
                return new OperationResult { Success = false, Message = "La hora de inicio debe ser anterior a la hora de fin." };

            var existingResult = await _availabilityRepository.GetById(dto.Id);
            if (!existingResult.Success) return existingResult;
            var availability = (Availability)existingResult.Data!;

            availability.Day = dto.Day;
            availability.StartTime = dto.StartTime;
            availability.EndTime = dto.EndTime;

            var result = await _availabilityRepository.Update(availability);
            if (!result.Success) return result;
            result.Data = ToDto((Availability)result.Data!);
            return result;
        }

        public async Task<OperationResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new OperationResult { Success = false, Message = "El identificador de la disponibilidad no puede estar vacío." };
            return await _availabilityRepository.Delete(id);
        }

        public async Task<OperationResult> GetByDoctor(string doctorId)
        {
            var result = await _availabilityRepository.GetByDoctor(doctorId);
            if (!result.Success) return result;
            var availabilities = (List<Availability>)result.Data!;
            result.Data = availabilities.Select(ToDto).ToList();
            return result;
        }

        private static AvailabilityDto ToDto(Availability availability) => new AvailabilityDto
        {
            Id = availability.Id,
            DoctorId = availability.DoctorId,
            Day = availability.Day,
            StartTime = availability.StartTime,
            EndTime = availability.EndTime
        };
    }
}
