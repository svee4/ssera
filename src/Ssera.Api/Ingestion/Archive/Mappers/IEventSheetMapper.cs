using Google.Apis.Sheets.v4.Data;

namespace Ssera.Api.Ingestion.Archive.Mappers;

public interface IEventSheetMapper
{
    IEnumerable<Event> ParseEvents(Sheet sheet);
}

