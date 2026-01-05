using System.Net;
using Spc.Application.Common.Exceptions;

namespace Spc.Api.Middleware;

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.ContentType = "application/json";

            var (statusCode, message) = ex switch
            {
                NotFoundException nf => (HttpStatusCode.NotFound, nf.Message),
                ValidationException ve => (HttpStatusCode.BadRequest, ve.Message),
                ConflictException ce => (HttpStatusCode.Conflict, ce.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                error = message,
                status = context.Response.StatusCode,
                path = context.Request.Path.Value
            });
        }
    }
}
