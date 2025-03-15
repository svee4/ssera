using Google.Apis.Sheets.v4.Data;

namespace Ssera.Api.Ingestion.EventArchive.Mappers;

public interface IEventArchiveSheetMapper
{
    IEnumerable<Event> ParseEvents(Sheet sheet);
}

