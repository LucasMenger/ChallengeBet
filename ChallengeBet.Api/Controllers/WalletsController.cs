using ChallengeBet.Api.Security;
using ChallengeBet.Application.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController(IWalletService svc) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyWallet(CancellationToken ct)
    {
        var playerId = User.GetPlayerId();
        var wallet = await svc.GetMyWalletAsync(playerId, ct);
        return Ok(wallet);
    }

    [HttpGet("me/transactions")]
    public async Task<IActionResult> GetMyTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var playerId = User.GetPlayerId();
        var res = await svc.ListMyTransactionsAsync(playerId, page, pageSize, ct);
        return Ok(res);
    }
}