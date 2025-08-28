using ChallengeBet.Application.Players;
using ChallengeBet.Application.Players.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController(IPlayerService service) : ControllerBase
{
    /// <summary>Cria um novo jogador com carteira atrelada.</summary>
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterPlayerRequest req, CancellationToken ct)
    {
        var (player, wallet) = await service.RegisterAsync(req, ct);
        return CreatedAtAction(nameof(Register), new { id = player.Id }, new { player, wallet });
    }
}