namespace ChallengeBet.Application.Abstractions;

public interface IRandomProvider
{
    double NextUnitDouble(); // [0,1)
}