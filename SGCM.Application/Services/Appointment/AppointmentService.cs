using SGCM.Application.DTOs.Appointment;
using SGCM.Application.Interfaces;
using SGCM.Data.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;
using SGCM.Domain.Enums;

namespace SGCM.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
        }

        public async Task<OperationResult> GetAll()
        {
            var result = await _appointmentRepository.GetAll();
            if (!result.Success) return result;
            var appointments = (List<Appointment>)result.Data!;
            result.Data = appointments.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetById(string id)
        {
            var result = await _appointmentRepository.GetById(id);
            if (!result.Success) return result;
            result.Data = ToDto((Appointment)result.Data!);
            return result;
        }

        public async Task<OperationResult> Create(CreateAppointmentDto dto)
        {
            if (dto is null)
                return new OperationResult { Success = false, Message = "Los datos de la cita no pueden estar vacíos." };
            if (string.IsNullOrWhiteSpace(dto.PatientId))
                return new OperationResult { Success = false, Message = "El paciente no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.DoctorId))
                return new OperationResult { Success = false, Message = "El doctor no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return new OperationResult { Success = false, Message = "El motivo de la cita no puede estar vacío." };
            if (dto.DateTime <= DateTime.UtcNow)
                return new OperationResult { Success = false, Message = "La fecha de la cita no puede estar en el pasado." };

            var patientResult = await _patientRepository.GetById(dto.PatientId);
            if (!patientResult.Success)
                return new OperationResult { Success = false, Message = "El paciente especificado no existe." };

            var doctorResult = await _doctorRepository.GetById(dto.DoctorId);
            if (!doctorResult.Success)
                return new OperationResult { Success = false, Message = "El doctor especificado no existe." };

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                DateTime = dto.DateTime,
                Reason = dto.Reason,
                Status = AppointmentStatus.Pending
            };

            var result = await _appointmentRepository.Add(appointment);
            if (!result.Success) return result;
            result.Data = ToDto((Appointment)result.Data!);
            return result;
        }

        public async Task<OperationResult> Update(UpdateAppointmentDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
                return new OperationResult { Success = false, Message = "El identificador de la cita no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return new OperationResult { Success = false, Message = "El motivo de la cita no puede estar vacío." };
            if (dto.DateTime <= DateTime.UtcNow)
                return new OperationResult { Success = false, Message = "La fecha de la cita no puede estar en el pasado." };

            var existingResult = await _appointmentRepository.GetById(dto.Id);
            if (!existingResult.Success) return existingResult;
            var appointment = (Appointment)existingResult.Data!;

            appointment.DateTime = dto.DateTime;
            appointment.Reason = dto.Reason;

            var result = await _appointmentRepository.Update(appointment);
            if (!result.Success) return result;
            result.Data = ToDto((Appointment)result.Data!);
            return result;
        }

        public async Task<OperationResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new OperationResult { Success = false, Message = "El identificador de la cita no puede estar vacío." };
            return await _appointmentRepository.Delete(id);
        }

        public async Task<OperationResult> GetByPatient(string patientId)
        {
            var result = await _appointmentRepository.GetByPatient(patientId);
            if (!result.Success) return result;
            var appointments = (List<Appointment>)result.Data!;
            result.Data = appointments.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetByDoctor(string doctorId)
        {
            var result = await _appointmentRepository.GetByDoctor(doctorId);
            if (!result.Success) return result;
            var appointments = (List<Appointment>)result.Data!;
            result.Data = appointments.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> GetByStatus(AppointmentStatus status)
        {
            var result = await _appointmentRepository.GetByStatus(status);
            if (!result.Success) return result;
            var appointments = (List<Appointment>)result.Data!;
            result.Data = appointments.Select(ToDto).ToList();
            return result;
        }

        public async Task<OperationResult> ChangeStatus(ChangeAppointmentStatusDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
                return new OperationResult { Success = false, Message = "El identificador de la cita no puede estar vacío." };

            var result = await _appointmentRepository.ChangeStatus(dto.Id, dto.Status);
            if (!result.Success) return result;
            result.Data = ToDto((Appointment)result.Data!);
            return result;
        }

        private static AppointmentDto ToDto(Appointment appointment) => new AppointmentDto
        {
            Id = appointment.Id,
            DateTime = appointment.DateTime,
            Reason = appointment.Reason,
            Status = appointment.Status,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId
        };
    }
}
