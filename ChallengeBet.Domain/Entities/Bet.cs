namespace ChallengeBet.Domain.Entities;

public class Bet
{
    public long Id { get; set; }
    public long PlayerId { get; set; }
    public decimal Amount { get; set; }
    public decimal Multiplier { get; set; }
    public Enums.BetStatus Status { get; set; } = Enums.BetStatus.Pending;
    public decimal? Prize { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }

    public Player? Player { get; set; }
}