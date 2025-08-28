using ChallengeBet.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ChallengeBet.Infrastructure.Services;

public class RtpConfig(IConfiguration cfg) : IRtpConfig
{
    public decimal GetRtp() => decimal.TryParse(cfg["Rtp:Value"], out var v) ? v : 0.95m;
    public decimal GetDefaultMultiplier() => decimal.TryParse(cfg["Rtp:DefaultMultiplier"], out var v) ? v : 2.0m;
}