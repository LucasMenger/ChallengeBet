namespace ChallengeBet.Application.Players.Dtos;

public class PlayerDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}