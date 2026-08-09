using SGCM.Domain.Entities;

namespace SGCM.Data.Interfaces
{
    public interface IJwtTokenGenerator
    {
        (string Token, DateTime Expiration) GenerateToken(AppUser user, IEnumerable<string> roles);
    }
}
