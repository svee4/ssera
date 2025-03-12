using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Ssera.Api.Database;

namespace Ssera.Api.Features.History;

[Handler]
public sealed partial class AddHistory
{
    public sealed record Command
    {
        [GreaterThanOrEqual(0)]
        public required int TotalEvents { get; set; }

        public required TimeSpan TimeTaken { get; set; }

        public string? Message { get; set; }
    }

    /// <summary>
    /// Adds a history entry to the database. Returns true if adding was successful, otherwise false
    /// </summary>
    /// <param name="command"></param>
    /// <param name="dbContext"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async ValueTask<bool> HandleAsync(
        Command command,
        AppDbContext dbContext,
        CancellationToken token)
    {
        var entity = WorkerHistory.CreateNew(command.TotalEvents, command.TimeTaken, command.Message);
        _ = await dbContext.WorkerHistory.AddAsync(entity, token);
        return await dbContext.SaveChangesAsync(token) > 0;
    }
}
