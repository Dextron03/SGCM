using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCM.Application.DTOs.Doctor;
using SGCM.Application.Interfaces;
using SGCM.Domain.Core;

namespace SGCM.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/doctors")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _doctorService.GetAll();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _doctorService.GetById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("by-specialty/{id}")]
        public async Task<IActionResult> GetBySpecialty(string id)
        {
            var result = await _doctorService.GetBySpecialty(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentDoctor()
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

            var result = await _doctorService.GetByAppUserId(appUserId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
        {
            var result = await _doctorService.Create(dto);

            if (!result.Success)
                return BadRequest(result);

            var doctor = (DoctorDto)result.Data!;

            return CreatedAtAction(
                nameof(GetById),
                new { id = doctor.Id },
                result
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] UpdateDoctorDto dto)
        {
            dto.Id = id;

            var result = await _doctorService.Update(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _doctorService.Delete(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}