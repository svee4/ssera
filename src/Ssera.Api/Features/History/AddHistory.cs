using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;
using Ssera.Api.Data;

namespace Ssera.Api.Features.History;

[Handler]
public sealed partial class AddHistory
{
    [Validate]
    public sealed partial record Command(string WorkerName, string Message) 
        : IValidationTarget<Command>;

    /// <summary>
    /// Adds a history entry to the database. Returns true if adding was successful, otherwise false
    /// </summary>
    /// <param name="command"></param>
    /// <param name="dbContext"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async ValueTask<bool> HandleAsync(
        Command command,
        ApiDbContext dbContext,
        CancellationToken token)
    {
        var entity = WorkerHistory.CreateNew(command.WorkerName, DateTime.UtcNow, command.Message);
        _ = await dbContext.WorkerHistory.AddAsync(entity, token);
        return await dbContext.SaveChangesAsync(token) > 0;
    }
}
