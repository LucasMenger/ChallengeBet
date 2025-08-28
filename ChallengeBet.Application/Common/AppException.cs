namespace ChallengeBet.Application.Common;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }

    public AppException(string message, System.Net.HttpStatusCode statusCode, string code, Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = (int)statusCode;
        Code = code;
    }
}