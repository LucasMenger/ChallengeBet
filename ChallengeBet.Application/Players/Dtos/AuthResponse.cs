namespace ChallengeBet.Application.Players.Dtos;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public PlayerDto Player { get; set; } = null!;
    public WalletDto Wallet { get; set; } = null!;
}