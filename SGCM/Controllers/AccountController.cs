using Microsoft.AspNetCore.Mvc;
using SGCM.Application.DTOs.Account;
using SGCM.Application.Interfaces;

namespace SGCM.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var result = await _accountService.Register(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _accountService.Login(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
