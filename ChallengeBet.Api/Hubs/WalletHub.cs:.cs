using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChallengeBet.Api.Hubs;

[Authorize]
public class WalletHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var sub = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (long.TryParse(sub, out var playerId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(playerId));
        }
        await base.OnConnectedAsync();
    }

    internal static string GroupName(long playerId) => $"player-{playerId}";
}