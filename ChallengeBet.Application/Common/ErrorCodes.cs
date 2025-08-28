namespace ChallengeBet.Application.Common;

public static class ErrorCodes
{
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string CONCURRENCY_CONFLICT = "CONCURRENCY_CONFLICT";
    public const string UNEXPECTED_ERROR = "UNEXPECTED_ERROR";
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string WALLET_NOT_FOUND = "WALLET_NOT_FOUND";
    public const string BET_NOT_FOUND = "BET_NOT_FOUND";
    public const string BET_NOT_PENDING = "BET_NOT_PENDING";
    public const string INSUFFICIENT_FUNDS = "INSUFFICIENT_FUNDS";
    public const string MIN_BET_NOT_MET = "MIN_BET_NOT_MET";
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
}