namespace ChallengeBet.Application.Bets.Dtos;

public class PlaceBetRequest
{
    public decimal Amount { get; set; }
    public decimal? Multiplier { get; set; }    
    public bool AutoSettle { get; set; } = true; 
}