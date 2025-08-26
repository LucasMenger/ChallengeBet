namespace ChallengeBet.Domain.Entities;

public class Enums
{
    public enum BetStatus { Pending = 0, Won = 1, Lost = 2, Cancelled = 3 }
    public enum TransactionType { BetDebit = 0, PrizeCredit = 1, BonusCredit = 2, RefundCredit = 3, DepositCredit = 4 }
}