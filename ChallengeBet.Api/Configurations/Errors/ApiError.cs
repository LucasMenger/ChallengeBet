namespace ChallengeBet.Api.Configurations.Errors;

public class ApiError
{
    public int Status { get; set; }
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public string? TraceId { get; set; }
    public object? Details { get; set; }
}