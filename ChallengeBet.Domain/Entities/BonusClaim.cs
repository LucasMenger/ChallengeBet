namespace ChallengeBet.Domain.Entities;

public class BonusClaim
{
    public long Id { get; set; }
    public long PlayerId { get; set; }
    public long RuleId { get; set; }
    public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
}