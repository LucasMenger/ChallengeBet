namespace ChallengeBet.Application.Abstractions;

public interface IWalletNotifier
{
    Task NotifyBalanceChanged(long playerId, decimal balance, CancellationToken ct = default);
}