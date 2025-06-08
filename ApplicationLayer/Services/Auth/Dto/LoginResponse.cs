using System;

namespace ApplicationLayer.Services.Auth.Dto
{
    public record LoginResponse(string Token, DateTime ExpireAt);
}
