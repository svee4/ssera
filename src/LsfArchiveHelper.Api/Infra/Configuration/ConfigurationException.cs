using System.Diagnostics.CodeAnalysis;

namespace LsfArchiveHelper.Api.Infra.Configuration;

public sealed class ConfigurationException : Exception
{
	public ConfigurationException(string message) : base(message)
	{
	}

	public ConfigurationException()
	{
	}

	public ConfigurationException(string message, Exception innerException) : base(message, innerException)
	{
	}

	[DoesNotReturn]
	public static T Throw<T>() => throw new ConfigurationException();
	
	[DoesNotReturn]
	public static T Throw<T>(string message) => throw new ConfigurationException(message);

}
