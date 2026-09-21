using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Microsoft.EntityFrameworkCore;
using Ssera.Api.Data;

namespace Ssera.Api.Features.Images;

[Handler]
[MapGet("/api/images/tags")]
public static partial class GetImageTags
{
    private const int MaxResults = 500;

    [Validate]
    public sealed partial record Request : IValidationTarget<Request>
    {
        public string? Search { get; init; }
    }

    private static async ValueTask<IReadOnlyList<string>> HandleAsync(
        Request request,
        ApiDbContext dbContext,
        CancellationToken token)
    {
        var tags = dbContext.Set<ImageArchiveTag>()
            .Select(tag => tag.Tag)
            .Distinct();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            tags = tags.Where(tag => EF.Functions.Like(tag, $"%{request.Search}%"));
        }

        return await tags
            .OrderBy(tag => tag)
            .Take(MaxResults)
            .ToListAsync(token);
    }
}
