namespace ChallengeBet.Application.Bets.Dtos;

public class BetDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public decimal Multiplier { get; set; }
    public string Status { get; set; } = "";
    public decimal? Prize { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public class BetListFilter
{
    public string? Status { get; set; } 
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}