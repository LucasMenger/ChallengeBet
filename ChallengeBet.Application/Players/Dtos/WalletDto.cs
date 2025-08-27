namespace ChallengeBet.Application.Players.Dtos;

public class WalletDto
{
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "BRL";
}