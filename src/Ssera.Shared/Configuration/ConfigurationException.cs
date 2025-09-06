using System.Diagnostics.CodeAnalysis;

namespace Ssera.Shared.Configuration;

[SuppressMessage("Design",
    "CA1032:Implement standard exception constructors",
    Justification = "I want standardized error messages")]
public sealed class ConfigurationException : Exception
{
    private ConfigurationException(string message) : base(message) { }

    public static ConfigurationException CreateMissingKey(string key) =>
        new($"Expected configuration value '{key}' was not present in the configuration.");

    [DoesNotReturn]
    public static void ThrowMissingKey(string key) =>
        throw CreateMissingKey(key);

    public static ConfigurationException CreateUnparsable<T>(string key) where T : IParsable<T> =>
        new($"Unable to parse configuration value '{key}' into {typeof(T).FullName}.");

    [DoesNotReturn]
    public static void ThrowUnparsable<T>(string key) where T : IParsable<T> =>
        throw CreateUnparsable<T>(key);
}
