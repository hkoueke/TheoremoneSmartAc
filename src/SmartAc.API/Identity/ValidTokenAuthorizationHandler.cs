using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartAc.Persistence.Repositories;

namespace SmartAc.API.Identity;

internal sealed class ValidTokenAuthorizationHandler : AuthorizationHandler<ValidTokenRequirement>
{
    private readonly SmartAcContext _smartAcContext;
    public ValidTokenAuthorizationHandler(SmartAcContext smartAcContext)
    {
        _smartAcContext = smartAcContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidTokenRequirement requirement)
    {
        string? tokenId =
            context.User.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;

        var deviceSerialNumber = context.User.Identity?.Name;

        if (tokenId is null || deviceSerialNumber is null)
        {
            return;
        }

        var isTokenValid = await _smartAcContext.DeviceRegistrations.AnyAsync(reg =>
            reg.DeviceSerialNumber == deviceSerialNumber &&
            reg.TokenId == tokenId &&
            reg.Active);

        if (isTokenValid)
        {
            context.Succeed(requirement);
        }
    }
}