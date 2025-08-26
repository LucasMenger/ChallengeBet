namespace ChallengeBet.Domain.Entities;

public class BonusRule
{
    public long Id { get; set; }
    public long PointsThreshold { get; set; }
    public decimal RewardValue { get; set; }
}