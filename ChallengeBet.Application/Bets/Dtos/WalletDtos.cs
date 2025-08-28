namespace ChallengeBet.Application.Bets.Dtos;


public class WalletDto
{
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "BRL";
}

public class TransactionDto
{
    public long Id { get; set; }
    public string Type { get; set; } = "";
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; }
}