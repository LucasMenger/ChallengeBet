using ChallengeBet.Application.Bets.Dtos;
using ChallengeBet.Application.Common;

namespace ChallengeBet.Application.Bets;

public interface IBetService
{
    Task<BetDto> PlaceBetAsync(long playerId, PlaceBetRequest req, CancellationToken ct);
    Task<BetDto> CancelBetAsync(long playerId, long betId, CancellationToken ct);
    Task<PagedResult<BetDto>> ListMyBetsAsync(long playerId, BetListFilter filter, CancellationToken ct);
}