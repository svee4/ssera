using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Ssera.Shared.Data;

[JsonConverter(typeof(JsonStringEnumConverter<GroupMember>))]
public enum GroupMember
{
    Chaewon = 1,
    Sakura,
    Yunjin,
    Kazuha,
    Eunchae
}

public static class DataExtensions
{
    /// <summary>Gets the display name of the given <see cref="GroupMember"/>.</summary>
    /// <exception cref="UnreachableException" />
    public static string GetDisplayName(this GroupMember value)
        => value switch
        {
            GroupMember.Chaewon => "Chaewon",
            GroupMember.Sakura => "Sakura",
            GroupMember.Yunjin => "Yunjin",
            GroupMember.Kazuha => "Kazuha",
            GroupMember.Eunchae => "Eunchae",
            _ => throw new UnreachableException($"Unknown {nameof(GroupMember)} value '{value}'")
        };
}
