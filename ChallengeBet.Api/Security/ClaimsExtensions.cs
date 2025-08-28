using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChallengeBet.Api.Security;

public static class ClaimsExtensions
{
    public static long GetPlayerId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new InvalidOperationException("JWT sem 'sub'.");
        return long.Parse(sub);
    }
}