using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCM.Application.DTOs.Patient;
using SGCM.Application.Interfaces;
using SGCM.Domain.Core;

namespace SGCM.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/patients")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _patientService.GetAll();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _patientService.GetById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentPatient()
        {
            var appUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(appUserId))
            {
                return Unauthorized(new OperationResult
                {
                    Success = false,
                    Message = "No se pudo identificar al usuario autenticado."
                });
            }

            var result = await _patientService.GetByAppUserId(appUserId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("by-social-security/{socialSecurityNumber}")]
        public async Task<IActionResult> GetBySocialSecurityNumber(
            string socialSecurityNumber)
        {
            var result = await _patientService
                .GetBySocialSecurityNumber(socialSecurityNumber);

            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreatePatientDto dto)
        {
            var result = await _patientService.Create(dto);

            if (!result.Success)
                return BadRequest(result);

            var patient = (PatientDto)result.Data!;

            return CreatedAtAction(
                nameof(GetById),
                new { id = patient.Id },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] UpdatePatientDto dto)
        {
            dto.Id = id;

            var result = await _patientService.Update(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _patientService.Delete(id);

            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}