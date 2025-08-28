using ChallengeBet.Api.Security;
using ChallengeBet.Application.Bets;
using ChallengeBet.Application.Bets.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBet.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BetsController(IBetService svc) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Place([FromBody] PlaceBetRequest req, CancellationToken ct)
    {
        var playerId = User.GetPlayerId();
        var bet = await svc.PlaceBetAsync(playerId, req, ct);
        return CreatedAtAction(nameof(Place), new { id = bet.Id }, bet);
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel([FromRoute] long id, CancellationToken ct)
    {
        var playerId = User.GetPlayerId();
        var bet = await svc.CancelBetAsync(playerId, id, ct);
        return Ok(bet);
    }

    [HttpGet("me")]
    public async Task<IActionResult> ListMy([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var playerId = User.GetPlayerId();
        var res = await svc.ListMyBetsAsync(playerId, new BetListFilter { Status = status, Page = page, PageSize = pageSize }, ct);
        return Ok(res);
    }
}