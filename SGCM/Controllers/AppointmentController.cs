using Microsoft.AspNetCore.Mvc;
using SGCM.Application.DTOs.Appointment;
using SGCM.Application.Interfaces;
using SGCM.Domain.Core;
using SGCM.Domain.Enums;

namespace SGCM.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => ToActionResult(await _appointmentService.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id) => ToActionResult(await _appointmentService.GetById(id));

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(string patientId) => ToActionResult(await _appointmentService.GetByPatient(patientId));

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctor(string doctorId) => ToActionResult(await _appointmentService.GetByDoctor(doctorId));

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(AppointmentStatus status) => ToActionResult(await _appointmentService.GetByStatus(status));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
    {
        var result = await _appointmentService.Create(dto);
        return result.Success ? StatusCode(StatusCodes.Status201Created, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAppointmentDto dto)
    {
        dto.Id = id;
        return ToActionResult(await _appointmentService.Update(dto));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeAppointmentStatusDto dto)
    {
        dto.Id = id;
        return ToActionResult(await _appointmentService.ChangeStatus(dto));
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(string id) => ToActionResult(await _appointmentService.ChangeStatus(new ChangeAppointmentStatusDto
    {
        Id = id,
        Status = AppointmentStatus.Canceled
    }));

    private IActionResult ToActionResult(OperationResult result) => result.Success ? Ok(result) : BadRequest(result);
}
