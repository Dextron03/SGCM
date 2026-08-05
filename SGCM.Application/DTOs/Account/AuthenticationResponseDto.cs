using System;
using System.Collections.Generic;

namespace SGCM.Application.DTOs.Account
{
    public class AuthenticationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string JWToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
