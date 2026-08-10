using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCM.Application.DTOs.Specialty;
using SGCM.Application.Interfaces;
using SGCM.Domain.Constants;

namespace SGCM.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/specialties")]
    public class SpecialtyController : ControllerBase
    {
        private readonly ISpecialtyService _specialtyService;

        public SpecialtyController(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _specialtyService.GetAll();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _specialtyService.GetById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Create([FromBody] CreateSpecialtyDto dto)
        {
            var result = await _specialtyService.Create(dto);

            if (!result.Success)
                return BadRequest(result);

            var specialty = (SpecialtyDto)result.Data!;

            return CreatedAtAction(
                nameof(GetById),
                new { id = specialty.Id },
                result
            );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Update(
            string id,
            [FromBody] UpdateSpecialtyDto dto)
        {
            dto.Id = id;

            var result = await _specialtyService.Update(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _specialtyService.Delete(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}