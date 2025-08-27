namespace ChallengeBet.Domain.Entities;

public class Wallet
{
    public long Id { get; set; }
    public long PlayerId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "BRL";
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Player? Player { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}