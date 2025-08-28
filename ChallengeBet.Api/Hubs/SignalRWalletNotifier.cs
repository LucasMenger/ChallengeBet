using ChallengeBet.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ChallengeBet.Api.Hubs;

public class SignalRWalletNotifier(IHubContext<WalletHub> hub) : IWalletNotifier
{
    public Task NotifyBalanceChanged(long playerId, decimal balance, CancellationToken ct = default)
        => hub.Clients.Group(WalletHub.GroupName(playerId)).SendAsync("walletUpdated", new { balance }, ct);
}