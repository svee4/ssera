using Google.Apis.Sheets.v4.Data;

namespace Ssera.Api.Worker.Mappers;

public interface IMapper
{
	string SheetName { get; }
	IEnumerable<Event> ParseEvents(Sheet sheet);

	public static IReadOnlyList<KnownSheet> KnownSheets { get; } =
	[
		new("Teasers/MV", DefaultMapper.Instance),
		new("Performance", PerformanceMapper.Instance),
		new("Music Shows", MusicShowsMapper.Instance),
		new("Behind The Scene", DefaultMapper.Instance),
		new("Interview", DefaultMapper.Instance),
		new("Variety", VarietyMapper.Instance),
		new("Reality", RealityMapper.Instance),
		new("CF", CFMapper.Instance),
		new("Misc", DefaultMapper.Instance),
		new("Mubank President", DefaultMapper.Instance),
		new("Weverse Live", WeverseMapper.Instance),
	];
}

public record KnownSheet(string Name, IMapper Mapper);

public interface IMapper<TImpl> 
	: IMapper
	where TImpl : IMapper<TImpl>
{
	static virtual TImpl Instance => throw new InvalidOperationException();
}
