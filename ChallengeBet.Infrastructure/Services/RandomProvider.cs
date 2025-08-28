using System.Security.Cryptography;
using ChallengeBet.Application.Abstractions;

namespace ChallengeBet.Infrastructure.Services;

public class RandomProvider : IRandomProvider
{
    public double NextUnitDouble()
    {
        Span<byte> b = stackalloc byte[8];
        RandomNumberGenerator.Fill(b);
        ulong ul = BitConverter.ToUInt64(b);
        
        return (ul / (double)ulong.MaxValue);
    }
}