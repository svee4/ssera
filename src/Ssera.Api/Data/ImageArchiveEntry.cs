using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssera.Api.Data;

public sealed class ImageArchiveEntry
{
    public int Id { get; private set; }
    public string FileId { get; private set; } = null!;
    public GroupMember Member { get; private set; }
    public ImageArchive.TopLevelKind TopLevelKind { get; private set; }
    public DateTime Date { get; private set; }
    public ICollection<ImageArchiveTag> Tags { get; private set; } = null!;

    private ImageArchiveEntry() { }

    public static ImageArchiveEntry Create(
        string fileId,
        GroupMember member,
        ImageArchive.TopLevelKind topLevelKind,
        DateTime date,
        IEnumerable<ImageArchiveTag> tags)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileId);

        return new ImageArchiveEntry
        {
            FileId = fileId,
            Member = member,
            TopLevelKind = topLevelKind,
            Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
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
