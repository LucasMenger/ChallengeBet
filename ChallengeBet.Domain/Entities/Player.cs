namespace ChallengeBet.Domain.Entities;

public class Player
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Wallet? Wallet { get; set; }
    public ICollection<Bet> Bets { get; set; } = new List<Bet>();
}