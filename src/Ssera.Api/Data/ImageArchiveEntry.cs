using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssera.Shared.Data;

namespace Ssera.Api.Data;

public sealed class ImageArchiveEntry
{
    public int Id { get; private set; }

    /// <summary>Google drive file id</summary>
    public string FileId { get; private set; } = null!;

    /// <summary>Member who's archive the image is from.</summary>
    public GroupMember Member { get; private set; }

    public ImageArchive.TopLevelKind TopLevelKind { get; private set; }

    /// <summary>Image width in pixels.</summary>
    public int Width { get; private set; }

    /// <summary>Image height in pixels.</summary>
    public int Height { get; private set; }

    /// <summary>
    ///     Date when the image was taken or published, in UTC.
    /// </summary>
    public DateTime Date
    {
        get => DateTime.SpecifyKind(_date, DateTimeKind.Utc);
        private set => _date = value;
    }

    private DateTime _date;

    public ICollection<ImageArchiveTag> Tags { get; private set; } = null!;

    private ImageArchiveEntry() { }

    public static ImageArchiveEntry Create(
        string fileId,
        GroupMember member,
        ImageArchive.TopLevelKind topLevelKind,
        DateTime date,
        int width,
        int height,
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
            Date = date,
            Width = width,
            Height = height,
            Tags = [.. tags]
        };
    }

    private sealed class Configuration : IEntityTypeConfiguration<ImageArchiveEntry>
    {
        public void Configure(EntityTypeBuilder<ImageArchiveEntry> builder)
        {
            builder.HasIndex(m => m.Date);
            builder.HasIndex(m => m.Member);
            builder.HasIndex(m => m.TopLevelKind);
        }
    }
}
