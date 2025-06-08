using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayer.Services.Auth.Dto;

namespace ApplicationLayer.Services.Auth
{
    public interface IJwtAuthService
    {
        bool ValidateUser(string username, string password, out string role);
        LoginResponse GenerateToken(string username, string role);
    }
}

