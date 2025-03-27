using System.Text.Json.Serialization;

namespace Ssera.Api.Data;

[JsonConverter(typeof(JsonStringEnumConverter<GroupMember>))]
public enum GroupMember
{
    Chaewon = 1,
    Sakura,
    Yunjin,
    Kazuha,
    Eunchae
}
