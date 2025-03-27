using Microsoft.AspNetCore.Mvc;

namespace Ssera.Api.Infra.ExceptionHandling;

public interface IProblemDetailsExceptionHandler<TException> where TException : Exception
{
    Task<ProblemDetails> Handle(
        HttpContext context,
        TException exception,
        CancellationToken token);
}
