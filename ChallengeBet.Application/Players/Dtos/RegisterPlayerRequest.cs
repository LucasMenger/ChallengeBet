namespace ChallengeBet.Application.Players.Dtos;

public class RegisterPlayerRequest
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public decimal? InitialBalance { get; set; }  
    public string Currency { get; set; } = "BRL";
}