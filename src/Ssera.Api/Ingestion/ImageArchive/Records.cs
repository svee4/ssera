using Ssera.Api.Data;
using System.Collections.Immutable;
using static Ssera.Api.Data.ImageArchive;

namespace Ssera.Api.Ingestion.ImageArchive;

public sealed record ImageArchiveInfo(GroupMember Member, string DriveId);

public sealed record ArchiveEntry(GroupMember Member, ImmutableArray<TopLevelEntry> TopLevelEntries);
public sealed record TopLevelEntry(TopLevelKind Kind, ImmutableArray<SubLevelEntry> SubLevelEntries);
public sealed record SubLevelEntry(DateTime Date, string Name, ImmutableArray<Entry> Entries);
public sealed record Entry(string Name, string Filename, string FileId, ImmutableArray<string> Tags);

