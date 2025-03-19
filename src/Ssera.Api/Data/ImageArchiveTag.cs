namespace Ssera.Api.Data;

public sealed class ImageArchiveTag
{
    public int Id { get; private set; }
    public string Tag { get; private set; } = null!;

    private ImageArchiveTag() { }

    public static ImageArchiveTag Create(string tag)
    {
        ArgumentException.ThrowIfNullOrEmpty(tag);

        return new ImageArchiveTag
        {
            Tag = tag
        };
    }
}
