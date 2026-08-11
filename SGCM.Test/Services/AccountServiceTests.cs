using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using SGCM.Application.DTOs.Account;
using SGCM.Application.Services;
using SGCM.Data.Interfaces;
using SGCM.Domain.Constants;
using SGCM.Domain.Entities;
using SGCM.Domain.Settings;
using Xunit;

namespace SGCM.Test.Services
{
    public class AccountServiceTests
    {
        private static (Mock<UserManager<AppUser>> userManager, Mock<IJwtTokenGenerator> tokenGenerator, Mock<IEmailSender> emailSender, AccountService service) CreateService()
        {
            var store = new Mock<IUserStore<AppUser>>();
            var userManager = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            var tokenGenerator = new Mock<IJwtTokenGenerator>();
            tokenGenerator
                .Setup(t => t.GenerateToken(It.IsAny<AppUser>(), It.IsAny<IEnumerable<string>>()))
                .Returns(("fake-jwt-token", DateTime.UtcNow.AddHours(1)));

            var emailSender = new Mock<IEmailSender>();
            var frontendSettings = Options.Create(new FrontendSettings { BaseUrl = "http://localhost:5173" });

            var service = new AccountService(userManager.Object, tokenGenerator.Object, emailSender.Object, frontendSettings);
            return (userManager, tokenGenerator, emailSender, service);
        }

        private static RegisterRequestDto ValidRegisterDto() => new()
        {
            FullName = "Juan Perez",
            Email = "juan.perez@example.com",
            PhoneNumber = "8095551234",
            Password = "Str0ng!Pass",
            ConfirmPassword = "Str0ng!Pass",
            Role = AppRoles.Patient
        };

        [Fact]
        public async Task Register_ValidDto_ShouldSendConfirmationEmail()
        {
            var (userManager, _, emailSender, service) = CreateService();
            var dto = ValidRegisterDto();

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);
            userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), dto.Password)).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), dto.Role)).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>())).ReturnsAsync("fake-confirmation-token");

            var result = await service.Register(dto);

            Assert.True(result.Success);
            emailSender.Verify(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Register_PasswordsDontMatch_ShouldFail()
        {
            var (_, _, _, service) = CreateService();
            var dto = ValidRegisterDto();
            dto.ConfirmPassword = "OtherPassword!";

            var result = await service.Register(dto);

            Assert.False(result.Success);
            Assert.Equal("Las contraseñas no coinciden.", result.Message);
        }

        [Fact]
        public async Task Register_InvalidRole_ShouldFail()
        {
            var (_, _, _, service) = CreateService();
            var dto = ValidRegisterDto();
            dto.Role = "SuperAdmin";

            var result = await service.Register(dto);

            Assert.False(result.Success);
            Assert.Equal("El rol especificado no es válido.", result.Message);
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ShouldFail()
        {
            var (userManager, _, _, service) = CreateService();
            var dto = ValidRegisterDto();

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(new AppUser { Email = dto.Email });

            var result = await service.Register(dto);

            Assert.False(result.Success);
            Assert.Equal("Ya existe una cuenta con ese correo electrónico.", result.Message);
        }

        [Fact]
        public async Task Register_RoleAssignmentFails_ShouldDeleteUserAndFail()
        {
            var (userManager, _, _, service) = CreateService();
            var dto = ValidRegisterDto();

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);
            userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), dto.Password)).ReturnsAsync(IdentityResult.Success);
            userManager
                .Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), dto.Role))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "No se pudo asignar el rol." }));
            userManager.Setup(m => m.DeleteAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);

            var result = await service.Register(dto);

            Assert.False(result.Success);
            userManager.Verify(m => m.DeleteAsync(It.IsAny<AppUser>()), Times.Once);
        }

        [Fact]
        public async Task Login_ValidCredentials_ShouldReturnSuccessWithToken()
        {
            var (userManager, _, _, service) = CreateService();
            var dto = new LoginRequestDto { Email = "juan.perez@example.com", Password = "Str0ng!Pass" };
            var user = new AppUser { Email = dto.Email, FullName = "Juan Perez", IsActive = true, EmailConfirmed = true };

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            userManager.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { AppRoles.Patient });

            var result = await service.Login(dto);

            Assert.True(result.Success);
            var response = (AuthenticationResponseDto)result.Data!;
            Assert.Equal("fake-jwt-token", response.JWToken);
            Assert.Contains(AppRoles.Patient, response.Roles);
        }

        [Fact]
        public async Task Login_UserNotFound_ShouldReturnGenericError()
        {
            var (userManager, _, _, service) = CreateService();
            var dto = new LoginRequestDto { Email = "unknown@example.com", Password = "whatever" };

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);

            var result = await service.Login(dto);

            Assert.False(result.Success);
            Assert.Equal("Credenciales inválidas.", result.Message);
        }

        [Fact]
        public async Task Login_WrongPassword_ShouldReturnGenericError()
        {
            var (userManager, _, _, service) = CreateService();
            var dto = new LoginRequestDto { Email = "juan.perez@example.com", Password = "WrongPassword" };
            var user = new AppUser { Email = dto.Email, IsActive = true, EmailConfirmed = true };

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            userManager.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

            var result = await service.Login(dto);

            Assert.False(result.Success);
            Assert.Equal("Credenciales inválidas.", result.Message);
        }

        [Fact]
        public async Task Login_EmailNotConfirmed_ShouldFail()
        {
            var (userManager, _, _, service) = CreateService();
            var dto = new LoginRequestDto { Email = "juan.perez@example.com", Password = "Str0ng!Pass" };
            var user = new AppUser { Email = dto.Email, IsActive = true, EmailConfirmed = false };

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            userManager.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

            var result = await service.Login(dto);

            Assert.False(result.Success);
            Assert.Equal("Debes confirmar tu correo electrónico antes de iniciar sesión.", result.Message);
        }

        [Fact]
        public async Task Login_InactiveUser_ShouldFail()
        {
            var (userManager, _, _, service) = CreateService();
            var dto = new LoginRequestDto { Email = "juan.perez@example.com", Password = "Str0ng!Pass" };
            var user = new AppUser { Email = dto.Email, IsActive = false, EmailConfirmed = true };

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            userManager.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);

            var result = await service.Login(dto);

            Assert.False(result.Success);
            Assert.Equal("La cuenta está inactiva.", result.Message);
        }

        [Fact]
        public async Task ForgotPassword_UserExists_ShouldSendResetEmail()
        {
            var (userManager, _, emailSender, service) = CreateService();
            var dto = new ForgotPasswordRequestDto { Email = "juan.perez@example.com" };
            var user = new AppUser { Id = "user-1", Email = dto.Email, FullName = "Juan Perez" };

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            userManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("fake-reset-token");

            var result = await service.ForgotPassword(dto);

            Assert.True(result.Success);
            emailSender.Verify(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_UserDoesNotExist_ShouldReturnGenericMessageWithoutSendingEmail()
        {
            var (userManager, _, emailSender, service) = CreateService();
            var dto = new ForgotPasswordRequestDto { Email = "unknown@example.com" };

            userManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);

            var result = await service.ForgotPassword(dto);

            Assert.True(result.Success);
            emailSender.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPassword_EmptyEmail_ShouldFail()
        {
            var (_, _, _, service) = CreateService();

            var result = await service.ForgotPassword(new ForgotPasswordRequestDto { Email = "" });

            Assert.False(result.Success);
        }

        [Fact]
        public async Task ResetPassword_Valid_ShouldSucceed()
        {
            var (userManager, _, _, service) = CreateService();
            var user = new AppUser { Id = "user-1" };
            var dto = new ResetPasswordRequestDto { UserId = "user-1", Token = "fake-token", NewPassword = "N3wStr0ng!Pass", ConfirmPassword = "N3wStr0ng!Pass" };

            userManager.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            userManager.Setup(m => m.ResetPasswordAsync(user, dto.Token, dto.NewPassword)).ReturnsAsync(IdentityResult.Success);

            var result = await service.ResetPassword(dto);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ResetPassword_PasswordsDontMatch_ShouldFail()
        {
            var (_, _, _, service) = CreateService();
            var dto = new ResetPasswordRequestDto { UserId = "user-1", Token = "fake-token", NewPassword = "N3wStr0ng!Pass", ConfirmPassword = "Different!Pass" };

            var result = await service.ResetPassword(dto);

            Assert.False(result.Success);
            Assert.Equal("Las contraseñas no coinciden.", result.Message);
        }

        [Fact]
        public async Task ResetPassword_UserNotFound_ShouldFail()
        {
            var (userManager, _, _, service) = CreateService();
            var dto = new ResetPasswordRequestDto { UserId = "missing", Token = "fake-token", NewPassword = "N3wStr0ng!Pass", ConfirmPassword = "N3wStr0ng!Pass" };

            userManager.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync((AppUser?)null);

            var result = await service.ResetPassword(dto);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task ResetPassword_InvalidToken_ShouldFail()
        {
            var (userManager, _, _, service) = CreateService();
            var user = new AppUser { Id = "user-1" };
            var dto = new ResetPasswordRequestDto { UserId = "user-1", Token = "expired-token", NewPassword = "N3wStr0ng!Pass", ConfirmPassword = "N3wStr0ng!Pass" };

            userManager.Setup(m => m.FindByIdAsync(dto.UserId)).ReturnsAsync(user);
            userManager
                .Setup(m => m.ResetPasswordAsync(user, dto.Token, dto.NewPassword))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Token inválido." }));

            var result = await service.ResetPassword(dto);

            Assert.False(result.Success);
        }
    }
}
