using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApplicationLayer.Services.Auth.Dto;

namespace ApplicationLayer.Services.Auth
{
    public class JwtAuthService : IJwtAuthService
    {
        private readonly IConfiguration _cfg;
        private readonly byte[] _key;

        public JwtAuthService(IConfiguration cfg)
        {
            _cfg = cfg;
            _key = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!);
        }

        private readonly Dictionary<string, (string Pwd, string Role)> _users = new()
        {
            ["admin"] = ("admin123", "Administrator"),
            ["user"] = ("user123", "User")
        };

        public bool ValidateUser(string user, string pwd, out string role)
        {
            role = string.Empty;
            if (!_users.TryGetValue(user, out var data) || data.Pwd != pwd)
                return false;
            role = data.Role;
            return true;
        }

        public LoginResponse GenerateToken(string user, string role)
        {
            var now = DateTime.UtcNow;
            var exp = now.AddMinutes(int.Parse(_cfg["Jwt:ExpireMinutes"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                notBefore: now,
                expires: exp,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(_key),
                    SecurityAlgorithms.HmacSha256));

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return new LoginResponse(tokenStr, exp);
        }
    }
}

