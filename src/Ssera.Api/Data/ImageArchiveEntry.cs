namespace Ssera.Api.Data;

public sealed class ImageArchiveEntry
{
    public int Id { get; private set; }
    public string FileId { get; private set; } = null!;
    public GroupMember Member { get; private set; }
    public ImageArchive.TopLevelKind TopLevelKind { get; private set; }

    /// <summary>
    /// UTC
    /// </summary>
    public DateTime Date => DateTime.SpecifyKind(_date, DateTimeKind.Utc);

    private DateTime _date;

    public ICollection<ImageArchiveTag> Tags { get; private set; } = null!;

    private ImageArchiveEntry() { }

    public static ImageArchiveEntry Create(
        string fileId,
        GroupMember member,
        ImageArchive.TopLevelKind topLevelKind,
        DateTime date,
        IEnumerable<ImageArchiveTag> tags)
    {
        if (date.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("DateTime Kind must be UTC");
        }

        ArgumentException.ThrowIfNullOrEmpty(fileId);

        return new ImageArchiveEntry
        {
            FileId = fileId,
            Member = member,
            TopLevelKind = topLevelKind,
            _date = date,
            Tags = [.. tags]
        };
    }

    //private sealed class Configuration : IEntityTypeConfiguration<ImageArchiveEntry>
    //{
    //    public void Configure(EntityTypeBuilder<ImageArchiveEntry> builder)
    //    {
    //        builder.Property(p => p.Tags)
    //            .
    //    }
    //}
}
