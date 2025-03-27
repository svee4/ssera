using Immediate.Validations.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Ssera.Api.Infra.ExceptionHandling;

public sealed class ValidationExceptionHandler : IProblemDetailsExceptionHandler<ValidationException>
{
    public Task<ProblemDetails> Handle(
        HttpContext context,
        ValidationException exception,
        CancellationToken token) => Task.FromResult<ProblemDetails>(
            new ValidationProblemDetails
            {
                Status = 400,
                Title = "Validation problem occurred",
                Detail = exception.Message,
                Errors = exception.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(y => y.ErrorMessage).ToArray())
            });
}
