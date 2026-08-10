using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCM.Application.DTOs.MedicalRecord;
using SGCM.Application.Interfaces;
using SGCM.Domain.Core;

namespace SGCM.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/medical-records")]
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _medicalRecordService.GetAll();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _medicalRecordService.GetById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(string patientId)
        {
            var result = await _medicalRecordService.GetByPatient(patientId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetByAppointment(string appointmentId)
        {
            var result = await _medicalRecordService.GetByAppointment(appointmentId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateMedicalRecordDto dto)
        {
            var result = await _medicalRecordService.Create(dto);

            if (!result.Success)
                return BadRequest(result);

            var medicalRecord = (MedicalRecordDto)result.Data!;

            return CreatedAtAction(
                nameof(GetById),
                new { id = medicalRecord.Id },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] UpdateMedicalRecordDto dto)
        {
            dto.Id = id;

            var result = await _medicalRecordService.Update(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _medicalRecordService.Delete(id);

            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
