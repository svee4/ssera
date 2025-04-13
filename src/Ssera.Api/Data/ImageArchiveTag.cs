using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssera.Api.Data;

// tag has to be a separate entity
// otherwise sqlite explodes trying to do the queries we need
public sealed class ImageArchiveTag
{
    public int Id { get; private set; }
    public string Tag { get; private set; } = null!;

    public ImageArchiveEntry Entry { get; private set; } = null!;

    private ImageArchiveTag() { }

    public static ImageArchiveTag Create(string tag)
    {
        ArgumentException.ThrowIfNullOrEmpty(tag);

        return new ImageArchiveTag
        {
            Tag = tag
        };
    }

    private sealed class Configuration : IEntityTypeConfiguration<ImageArchiveTag>
    {
        public void Configure(EntityTypeBuilder<ImageArchiveTag> builder)
        {
            builder.HasIndex(m => m.Tag);
        }
    }
}
