using SGCM.Application.DTOs.Specialty;
using SGCM.Application.Interfaces;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;

namespace SGCM.Application.Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly ISpecialtyRepository _specialtyRepository;

        public SpecialtyService(ISpecialtyRepository specialtyRepository)
        {
            _specialtyRepository = specialtyRepository;
        }

        public async Task<OperationResult> GetAll()
        {
            var result = await _specialtyRepository.GetAll();
            if (!result.Success) return result;
            var specialties = (List<Specialty>)result.Data!;
            result.Data = specialties.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetById(string id)
        {
            var result = await _specialtyRepository.GetById(id);
            if (!result.Success) return result;
            result.Data = ToDto((Specialty)result.Data!);
            return result;
        }

        public async Task<OperationResult> Create(CreateSpecialtyDto dto)
        {
            if (dto is null)
                return new OperationResult { Success = false, Message = "Los datos de la especialidad no pueden estar vacíos." };
            if (string.IsNullOrWhiteSpace(dto.Name))
                return new OperationResult { Success = false, Message = "El nombre de la especialidad no puede estar vacío." };

            var nameExists = await _specialtyRepository.Exists(s => s.Name == dto.Name);
            if (nameExists)
                return new OperationResult { Success = false, Message = "Ya existe una especialidad con ese nombre." };

            var specialty = new Specialty
            {
                Name = dto.Name,
                Description = dto.Description
            };

            var result = await _specialtyRepository.Add(specialty);
            if (!result.Success) return result;
            result.Data = ToDto((Specialty)result.Data!);
            return result;
        }

        public async Task<OperationResult> Update(UpdateSpecialtyDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
                return new OperationResult { Success = false, Message = "El identificador de la especialidad no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Name))
                return new OperationResult { Success = false, Message = "El nombre de la especialidad no puede estar vacío." };

            var existingResult = await _specialtyRepository.GetById(dto.Id);
            if (!existingResult.Success) return existingResult;
            var specialty = (Specialty)existingResult.Data!;

            if (specialty.Name != dto.Name)
            {
                var duplicated = await _specialtyRepository.Exists(s => s.Name == dto.Name && s.Id != dto.Id);
                if (duplicated)
                    return new OperationResult { Success = false, Message = "Ya existe otra especialidad con ese nombre." };
            }

            specialty.Name = dto.Name;
            specialty.Description = dto.Description;

            var result = await _specialtyRepository.Update(specialty);
            if (!result.Success) return result;
            result.Data = ToDto((Specialty)result.Data!);
            return result;
        }

        public async Task<OperationResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new OperationResult { Success = false, Message = "El identificador de la especialidad no puede estar vacío." };
            return await _specialtyRepository.Delete(id);
        }

        private static SpecialtyDto ToDto(Specialty specialty) => new SpecialtyDto
        {
            Id = specialty.Id,
            Name = specialty.Name,
            Description = specialty.Description
        };
    }
}
