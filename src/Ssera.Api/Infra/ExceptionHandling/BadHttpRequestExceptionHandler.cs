using Microsoft.AspNetCore.Mvc;

namespace Ssera.Api.Infra.ExceptionHandling;

public sealed class BadHttpRequestExceptionHandler : IProblemDetailsExceptionHandler<BadHttpRequestException>
{
    public Task<ProblemDetails> Handle(
        HttpContext context,
        BadHttpRequestException exception,
        CancellationToken token) => Task.FromResult<ProblemDetails>(
            new ValidationProblemDetails
            {
                Status = 400,
                Title = "Bad request",
                Detail = exception.InnerException?.Message ?? exception.Message
            });
}
