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
        private readonly IAvailabilityRepository _availabilityRepository;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IAvailabilityRepository availabilityRepository)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _availabilityRepository = availabilityRepository;
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

            var scheduleValidation = await ValidateSchedule(dto.DoctorId, dto.DateTime);
            if (scheduleValidation is not null) return scheduleValidation;

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

            var scheduleValidation = await ValidateSchedule(appointment.DoctorId, dto.DateTime, appointment.Id);
            if (scheduleValidation is not null) return scheduleValidation;

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

            if (!Enum.IsDefined(dto.Status))
                return new OperationResult { Success = false, Message = "El estado de la cita no es válido." };

            var appointmentResult = await _appointmentRepository.GetById(dto.Id);
            if (!appointmentResult.Success) return appointmentResult;
            var appointment = (Appointment)appointmentResult.Data!;
            if (!CanChangeStatus(appointment.Status, dto.Status))
                return new OperationResult { Success = false, Message = "No se permite cambiar la cita a ese estado." };

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

        private static AvailableDay ToAvailableDay(DateTime dateTime) =>
            (AvailableDay)(((int)dateTime.DayOfWeek + 6) % 7 + 1);

        private static bool CanChangeStatus(AppointmentStatus current, AppointmentStatus next) =>
            current switch
            {
                AppointmentStatus.Pending => next is AppointmentStatus.Confirmed or AppointmentStatus.Canceled,
                AppointmentStatus.Confirmed => next is AppointmentStatus.Completed or AppointmentStatus.Canceled,
                _ => false
            };

        private async Task<OperationResult?> ValidateSchedule(string doctorId, DateTime dateTime, string? excludedAppointmentId = null)
        {
            var availabilityResult = await _availabilityRepository.GetByDoctor(doctorId);
            if (!availabilityResult.Success) return availabilityResult;

            var appointmentDay = ToAvailableDay(dateTime);
            var appointmentTime = dateTime.TimeOfDay;
            var isWithinAvailability = ((List<Availability>)availabilityResult.Data!)
                .Any(a => a.Day == appointmentDay && appointmentTime >= a.StartTime && appointmentTime < a.EndTime);
            if (!isWithinAvailability)
                return new OperationResult { Success = false, Message = "El doctor no está disponible en la fecha y hora seleccionadas." };

            var doctorAppointmentsResult = await _appointmentRepository.GetByDoctor(doctorId);
            if (!doctorAppointmentsResult.Success) return doctorAppointmentsResult;
            var isAlreadyBooked = ((List<Appointment>)doctorAppointmentsResult.Data!)
                .Any(a => a.Id != excludedAppointmentId && a.DateTime == dateTime && a.Status != AppointmentStatus.Canceled);
            return isAlreadyBooked
                ? new OperationResult { Success = false, Message = "El horario seleccionado ya está ocupado." }
                : null;
        }
    }
}
