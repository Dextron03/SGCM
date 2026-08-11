using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IAvailabilityRepository availabilityRepository,
            UserManager<AppUser> userManager,
            IEmailSender emailSender)
        {
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _availabilityRepository = availabilityRepository;
            _userManager = userManager;
            _emailSender = emailSender;
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
            appointment = (Appointment)result.Data!;

            var patient = (Patient)patientResult.Data!;
            var doctor = (Doctor)doctorResult.Data!;

            await NotifyAsync(patient.AppUserId,
                "Solicitud de cita registrada - SGCM",
                $"<p>Tu solicitud de cita para el {FormatDateTime(appointment.DateTime)} fue registrada y está pendiente de confirmación por el médico.</p>");

            await NotifyAsync(doctor.AppUserId,
                "Nueva solicitud de cita - SGCM",
                $"<p>Tienes una nueva solicitud de cita para el {FormatDateTime(appointment.DateTime)}. Ingresa al sistema para confirmarla o rechazarla.</p>");

            result.Data = ToDto(appointment);
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

            if (appointment.Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed))
                return new OperationResult { Success = false, Message = "Solo se pueden reprogramar citas pendientes o confirmadas." };

            var scheduleValidation = await ValidateSchedule(appointment.DoctorId, dto.DateTime, appointment.Id);
            if (scheduleValidation is not null) return scheduleValidation;

            appointment.DateTime = dto.DateTime;
            appointment.Reason = dto.Reason;
            appointment.Status = AppointmentStatus.Pending;

            var result = await _appointmentRepository.Update(appointment);
            if (!result.Success) return result;
            appointment = (Appointment)result.Data!;

            await NotifyAppointmentPartiesAsync(appointment,
                "Cita reprogramada - SGCM",
                $"<p>La cita fue reprogramada para el {FormatDateTime(appointment.DateTime)}. Queda pendiente de confirmación por el médico.</p>");

            result.Data = ToDto(appointment);
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
            var previousStatus = appointment.Status;
            if (!CanChangeStatus(appointment.Status, dto.Status))
                return new OperationResult { Success = false, Message = "No se permite cambiar la cita a ese estado." };

            var result = await _appointmentRepository.ChangeStatus(dto.Id, dto.Status);
            if (!result.Success) return result;
            appointment = (Appointment)result.Data!;

            await NotifyStatusChangeAsync(appointment, previousStatus);

            result.Data = ToDto(appointment);
            return result;
        }

        private async Task NotifyStatusChangeAsync(Appointment appointment, AppointmentStatus previousStatus)
        {
            try
            {
                switch (appointment.Status)
                {
                    case AppointmentStatus.Confirmed:
                        var patientResult = await _patientRepository.GetById(appointment.PatientId);
                        if (patientResult?.Success == true)
                            await NotifyAsync(((Patient)patientResult.Data!).AppUserId,
                                "Cita confirmada - SGCM",
                                $"<p>Tu cita para el {FormatDateTime(appointment.DateTime)} fue confirmada por el médico.</p>");
                        break;

                    case AppointmentStatus.Canceled:
                        var reason = previousStatus == AppointmentStatus.Pending ? "rechazada" : "cancelada";
                        await NotifyAppointmentPartiesAsync(appointment,
                            $"Cita {reason} - SGCM",
                            $"<p>La cita para el {FormatDateTime(appointment.DateTime)} fue {reason}.</p>");
                        break;
                }
            }
            catch
            {
                // El envío de notificaciones no debe impedir que la operación sobre la cita se complete.
            }
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

        private async Task NotifyAppointmentPartiesAsync(Appointment appointment, string subject, string htmlBody)
        {
            try
            {
                var patientResult = await _patientRepository.GetById(appointment.PatientId);
                if (patientResult?.Success == true)
                    await NotifyAsync(((Patient)patientResult.Data!).AppUserId, subject, htmlBody);

                var doctorResult = await _doctorRepository.GetById(appointment.DoctorId);
                if (doctorResult?.Success == true)
                    await NotifyAsync(((Doctor)doctorResult.Data!).AppUserId, subject, htmlBody);
            }
            catch
            {
                // El envío de notificaciones no debe impedir que la operación sobre la cita se complete.
            }
        }

        private async Task NotifyAsync(string appUserId, string subject, string htmlBody)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(appUserId);
                if (user?.Email is null) return;

                await _emailSender.SendEmailAsync(user.Email, subject, htmlBody);
            }
            catch
            {
                // El envío de notificaciones no debe impedir que la operación sobre la cita se complete.
            }
        }

        private static string FormatDateTime(DateTime dateTime) => dateTime.ToString("dddd d 'de' MMMM 'a las' h:mm tt");
    }
}
