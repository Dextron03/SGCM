using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCM.Application.DTOs.Availability;
using SGCM.Application.Interfaces;
using SGCM.Domain.Constants;
using SGCM.Domain.Core;

namespace SGCM.Controllers;

[ApiController]
[Authorize]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> GetAll() => ToActionResult(await _availabilityService.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id) => ToActionResult(await _availabilityService.GetById(id));

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctor(string doctorId) => ToActionResult(await _availabilityService.GetByDoctor(doctorId));

    [HttpPost]
    [Authorize(Roles = AppRoles.Doctor + "," + AppRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateAvailabilityDto dto)
    {
        var result = await _availabilityService.Create(dto);
        return result.Success ? StatusCode(StatusCodes.Status201Created, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = AppRoles.Doctor + "," + AppRoles.Admin)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAvailabilityDto dto)
    {
        dto.Id = id;
        return ToActionResult(await _availabilityService.Update(dto));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = AppRoles.Doctor + "," + AppRoles.Admin)]
    public async Task<IActionResult> Delete(string id) => ToActionResult(await _availabilityService.Delete(id));

    private IActionResult ToActionResult(OperationResult result) => result.Success ? Ok(result) : BadRequest(result);
}
