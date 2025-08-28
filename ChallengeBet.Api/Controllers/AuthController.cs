using ChallengeBet.Application.Players;
using ChallengeBet.Application.Players.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBet.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController(IPlayerService service) : ControllerBase
{
    /// <summary>Login do jogador. Retorna JWT, dados do jogador e saldo.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var resp = await service.LoginAsync(req, ct);
        return Ok(resp);
    }
}