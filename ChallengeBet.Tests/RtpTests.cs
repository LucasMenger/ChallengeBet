using ChallengeBet.Application.Abstractions;
using ChallengeBet.Application.Bets.Dtos;
using ChallengeBet.Domain.Entities;
using ChallengeBet.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ChallengeBet.Tests;

public class RtpTests
{
    private sealed class FakeRng(double[] seq) : IRandomProvider
    {
        int i = 0; public double NextUnitDouble() => seq[i++ % seq.Length];
    }

    [Fact]
    public async Task Expected_Return_Should_Approximate_RTP()
    {
        using var db = DbTestHelper.CreateNewDb();
        var player = new Player { Name="T", Email="t@t.com", PasswordHash="x" };
        var wallet = new Wallet { Player=player, Balance=0m, Currency="BRL" };
        db.AddRange(player, wallet, new PlayerPoints{ PlayerId = player.Id });
        await db.SaveChangesAsync();

        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{
            ["Rtp:Value"] = "0.95",
            ["Rtp:DefaultMultiplier"] = "2.0"
        }).Build();

        var rng = new FakeRng(Enumerable.Range(0,1000).Select(i => i/1000.0).ToArray());
        var betSvc = new BetService(db, new RtpConfig(cfg), rng);

        decimal totalBet = 0, totalReturn = 0;
        for (int i=0;i<1000;i++)
        {
            var before = wallet.Balance;
            var res = await betSvc.PlaceBetAsync(player.Id,new PlaceBetRequest{Amount=10m,Multiplier=2m,AutoSettle=true},default);
            await db.Entry(wallet).ReloadAsync();
            var after = wallet.Balance;
            totalBet += 10m;
            totalReturn += (after - before + 10m);
        }
        var expected = 0.95m * totalBet;
        (totalReturn/totalBet).Should().BeApproximately(0.95m, 0.05m);
    }
}