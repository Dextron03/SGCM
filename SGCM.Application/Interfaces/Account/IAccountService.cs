using SGCM.Application.DTOs.Account;
using SGCM.Domain.Core;

namespace SGCM.Application.Interfaces
{
    public interface IAccountService
    {
        Task<OperationResult> Register(RegisterRequestDto dto);
        Task<OperationResult> Login(LoginRequestDto dto);
        Task<OperationResult> ConfirmEmail(string userId, string token);
        Task<OperationResult> ForgotPassword(ForgotPasswordRequestDto dto);
        Task<OperationResult> ResetPassword(ResetPasswordRequestDto dto);
    }
}
