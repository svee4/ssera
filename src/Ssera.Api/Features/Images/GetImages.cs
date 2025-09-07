using Immediate.Apis.Shared;
using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Microsoft.EntityFrameworkCore;
using Ssera.Api.Data;
using Ssera.Shared.Images;
using Ssera.Shared.Images.Filters;
using System.Diagnostics;

namespace Ssera.Api.Features.Images;

[Handler]
[MapGet("/api/images")]
public sealed partial class GetImages
{
    [Validate]
    public sealed partial record Request : IValidationTarget<Request>
    {
        public GetImagesQuery RequestParameters { get; init; } = null!;
    }

    private static async ValueTask<GetImagesResponse> HandleAsync(
        Request requestBase,
        ApiDbContext dbContext,
        CancellationToken token)
    {
        var request = requestBase.RequestParameters;

        var query = dbContext.ImageArchive.AsQueryable();

        if (request.TagsFilter is { } tagsFilter)
        {
            var tags = tagsFilter.Tags;

            query = tagsFilter.TagsSelectionType switch
            {
                TagsFilterType.Include => query.Where(entry =>
                    entry.Tags.Any(tag => tags.Contains(tag.Tag))),

                TagsFilterType.Exclude => query.Where(entry =>
                    !entry.Tags.Any(tag => tags.Contains(tag.Tag))),

                var v => throw new UnreachableException($"Unhandled '{nameof(TagsFilterType)}' value '{v}'"),
            };
        }

        if (request.Eras is { Length: > 0 } eras)
        {
            var dbEras = eras.Select(EraToTopLevelKind).ToArray();
            query = query.Where(e => dbEras.Contains(e.TopLevelKind));
        }

        if (request.Members is { Length: > 0 } members)
        {
            query = query.Where(e => members.Contains(e.Member));
        }

        var isDescending = request.Sort == SortType.Descending;
        var orderedQuery = (request.OrderBy, isDescending) switch
        {
            (OrderByType.Date, true) => query.OrderByDescending(entry => entry.Date),
            (OrderByType.Date, false) => query.OrderBy(entry => entry.Date),

            // i have no idea how to do this query properly in ef core
            // but the functionality we want is to first order by the first tag,
            // then if they are equal then the second tag etc etc
            (OrderByType.Tags, true) => query
                .OrderByDescending(entry => entry.Tags.FirstOrDefault())
                .ThenByDescending(entry => entry.Tags.ElementAtOrDefault(1))
                .ThenByDescending(entry => entry.Tags.ElementAtOrDefault(2)),

            (OrderByType.Tags, false) => query
                .OrderBy(entry => entry.Tags.FirstOrDefault())
                .ThenBy(entry => entry.Tags.ElementAtOrDefault(1))
                .ThenBy(entry => entry.Tags.ElementAtOrDefault(2)),
            _ => null
        };

        query = orderedQuery is not null
            ? orderedQuery.ThenByDescending(entry => entry.Id)
            : query.OrderByDescending(entry => entry.Id);

        var count = await query.CountAsync(token);

        var skips = (request.Page - 1) * request.PageSize;

        if (skips > count)
        {
            return new GetImagesResponse([], count);
        }

        query = query.Skip(skips).Take(request.PageSize);

        var resultsQuery = query.Select(entry => new GetImagesResponse.Image(
                entry.FileId,
                entry.Member,
                TopLevelKindToEra(entry.TopLevelKind),
                entry.Width,
                entry.Height,
                entry.Tags.Select(t => t.Tag).ToList(),
                entry.Date
        ));

        var results = await resultsQuery.ToListAsync(token);
        return new GetImagesResponse(results, count);
    }

    private static ImageArchive.TopLevelKind EraToTopLevelKind(Era era) =>
        era switch
        {
            Era.Fearless => ImageArchive.TopLevelKind.Fearless,
            Era.Antifragile => ImageArchive.TopLevelKind.Antifragile,
            Era.Unforgiven => ImageArchive.TopLevelKind.Unforgiven,
            Era.PerfectNight => ImageArchive.TopLevelKind.PerfectNight,
            Era.Easy => ImageArchive.TopLevelKind.Easy,
            Era.Crazy => ImageArchive.TopLevelKind.Crazy,
            _ => throw new UnreachableException()
        };

    private static Era? TopLevelKindToEra(ImageArchive.TopLevelKind kind) =>
        kind switch
        {
            ImageArchive.TopLevelKind.Fearless => Era.Fearless,
            ImageArchive.TopLevelKind.Antifragile => Era.Antifragile,
            ImageArchive.TopLevelKind.Unforgiven => Era.Unforgiven,
            ImageArchive.TopLevelKind.PerfectNight => Era.PerfectNight,
            ImageArchive.TopLevelKind.Easy => Era.Easy,
            ImageArchive.TopLevelKind.Crazy => Era.Crazy,
            _ => null
        };
}
