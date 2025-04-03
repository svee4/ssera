using System.Text.Json.Serialization;

namespace Ssera.Api.Features.Images;

partial class GetImages
{
    [JsonConverter(typeof(JsonStringEnumConverter<OrderByType>))]
    public enum OrderByType
    {
        Date = 1,
        Tags,
    }

    [JsonConverter(typeof(JsonStringEnumConverter<SortType>))]
    public enum SortType
    {
        Ascending = 1,
        Descending
    }

    [JsonConverter(typeof(JsonStringEnumConverter<Era>))]
    public enum Era
    {
        Fearless = 1,
        Antifragile,
        Unforgiven,
        PerfectNight,
        Easy,
        Crazy,
        Hot
    }
}
