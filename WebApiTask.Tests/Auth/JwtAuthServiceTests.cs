using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using ApplicationLayer.Services.Auth;
using Xunit;

namespace WebApiTask.Tests.Auth
{
    public class JwtAuthServiceTests
    {
        private readonly IConfiguration _cfg;
        private readonly JwtAuthService _svc;

        public JwtAuthServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Jwt:Key", "supersecretkey12345678901234567890"},
                {"Jwt:Issuer", "test"},
                {"Jwt:Audience", "test"},
                {"Jwt:ExpireMinutes", "60"}
            };
            _cfg = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
            _svc = new JwtAuthService(_cfg);
        }

        [Fact]
        public void ValidateUser_ReturnsTrue_ForValidCredentials()
        {
            var result = _svc.ValidateUser("admin", "admin123", out var role);
            Assert.True(result);
            Assert.Equal("Administrator", role);
        }

        [Fact]
        public void ValidateUser_ReturnsFalse_ForInvalidCredentials()
        {
            var result = _svc.ValidateUser("foo", "bar", out var role);
            Assert.False(result);
            Assert.Equal(string.Empty, role);
        }

        [Fact]
        public void GenerateToken_ReturnsTokenWithProperExpiration()
        {
            var resp = _svc.GenerateToken("admin", "Administrator");
            Assert.True(resp.ExpireAt > DateTime.UtcNow.AddMinutes(59));
            Assert.False(string.IsNullOrWhiteSpace(resp.Token));
        }
    }
}