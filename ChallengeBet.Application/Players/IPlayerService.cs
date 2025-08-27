using ChallengeBet.Application.Players.Dtos;

namespace ChallengeBet.Application.Players;

public interface IPlayerService
{
    Task<(PlayerDto player, WalletDto wallet)> RegisterAsync(RegisterPlayerRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
}