using Microsoft.AspNetCore.Identity;
using SGCM.Application.DTOs.Account;
using SGCM.Application.Interfaces;
using SGCM.Data.Interfaces;
using SGCM.Data.Validation;
using SGCM.Domain.Constants;
using SGCM.Domain.Core;
using SGCM.Domain.Entities;

namespace SGCM.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AccountService(UserManager<AppUser> userManager, IJwtTokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<OperationResult> Register(RegisterRequestDto dto)
        {
            if (dto is null)
                return new OperationResult { Success = false, Message = "Los datos de registro no pueden estar vacíos." };
            if (string.IsNullOrWhiteSpace(dto.FullName))
                return new OperationResult { Success = false, Message = "El nombre completo no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Email))
                return new OperationResult { Success = false, Message = "El correo electrónico no puede estar vacío." };
            if (string.IsNullOrWhiteSpace(dto.Password))
                return new OperationResult { Success = false, Message = "La contraseña no puede estar vacía." };
            if (!RegistroValidator.PasswordsMatch(dto.Password, dto.ConfirmPassword))
                return new OperationResult { Success = false, Message = "Las contraseñas no coinciden." };
            if (!AppRoles.SelfRegisterable.Contains(dto.Role))
                return new OperationResult { Success = false, Message = "El rol especificado no es válido." };

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
                return new OperationResult { Success = false, Message = "Ya existe una cuenta con ese correo electrónico." };

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(" ", createResult.Errors.Select(e => e.Description));
                return new OperationResult { Success = false, Message = errors };
            }

            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                var errors = string.Join(" ", roleResult.Errors.Select(e => e.Description));
                return new OperationResult { Success = false, Message = errors };
            }

            var (token, expiration) = _tokenGenerator.GenerateToken(user, new[] { dto.Role });

            return new OperationResult
            {
                Success = true,
                Data = ToAuthResponse(user, new List<string> { dto.Role }, token, expiration)
            };
        }

        public async Task<OperationResult> Login(LoginRequestDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return new OperationResult { Success = false, Message = "Credenciales inválidas." };

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return new OperationResult { Success = false, Message = "Credenciales inválidas." };

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
                return new OperationResult { Success = false, Message = "Credenciales inválidas." };

            if (!user.IsActive)
                return new OperationResult { Success = false, Message = "La cuenta está inactiva." };

            var roles = await _userManager.GetRolesAsync(user);
            var (token, expiration) = _tokenGenerator.GenerateToken(user, roles);

            return new OperationResult
            {
                Success = true,
                Data = ToAuthResponse(user, roles.ToList(), token, expiration)
            };
        }

        private static AuthenticationResponseDto ToAuthResponse(AppUser user, List<string> roles, string token, DateTime expiration) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            Roles = roles,
            JWToken = token,
            Expiration = expiration
        };
    }
}
