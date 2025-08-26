namespace ChallengeBet.Domain.Entities;

public class Transaction
{
    public long Id { get; set; }
    public long WalletId { get; set; }
    public long PlayerId { get; set; }
    public long? BetId { get; set; }
    public Enums.TransactionType Type { get; set; }
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Wallet? Wallet { get; set; }
    public Player? Player { get; set; }
}