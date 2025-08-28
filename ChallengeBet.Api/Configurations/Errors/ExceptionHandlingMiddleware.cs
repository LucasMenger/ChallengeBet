using System.Net;
using ChallengeBet.Application.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBet.Api.Configurations.Errors;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex, logger);
        }
    }

    private static async Task HandleAsync(HttpContext ctx, Exception ex, ILogger logger)
    {
        var traceId = ctx.TraceIdentifier;

        ApiError payload;
        int status;

        switch (ex)
        {
            case AppException appEx:
                status = appEx.StatusCode;
                payload = new ApiError { Status = status, Code = appEx.Code, Message = appEx.Message, TraceId = traceId };
                break;

            case ValidationException fvEx:
                status = (int)HttpStatusCode.BadRequest;
                payload = new ApiError
                {
                    Status = status,
                    Code = ErrorCodes.VALIDATION_ERROR,
                    Message = "Validation failed.",
                    TraceId = traceId,
                    Details = fvEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                };
                break;

            case DbUpdateConcurrencyException:
                status = (int)HttpStatusCode.Conflict;
                payload = new ApiError { Status = status, Code = ErrorCodes.CONCURRENCY_CONFLICT, Message = "Concurrency conflict.", TraceId = traceId };
                break;

            case InvalidOperationException inv:
                status = (int)HttpStatusCode.BadRequest;
                payload = new ApiError { Status = status, Code = ErrorCodes.UNEXPECTED_ERROR, Message = inv.Message, TraceId = traceId };
                break;

            default:
                status = (int)HttpStatusCode.InternalServerError;
                payload = new ApiError { Status = status, Code = ErrorCodes.UNEXPECTED_ERROR, Message = "Erro interno inesperado.", TraceId = traceId };
                break;
        }

        logger.LogError(ex, "Request failed. Code={Code} Status={Status} TraceId={TraceId}", payload.Code, status, traceId);

        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(payload);
    }
}