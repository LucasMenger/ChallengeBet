using ChallengeBet.Application.Bets.Dtos;
using ChallengeBet.Application.Common;
using WalletDto = ChallengeBet.Application.Players.Dtos.WalletDto;

namespace ChallengeBet.Application.Wallets;

public interface IWalletService
{
    Task<WalletDto> GetMyWalletAsync(long playerId, CancellationToken ct);
    Task<PagedResult<TransactionDto>> ListMyTransactionsAsync(long playerId, int page, int pageSize, CancellationToken ct);

}