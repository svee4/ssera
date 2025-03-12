namespace Ssera.Api.Infra.Configuration;

public static class ConfigurationExtensions
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="configuration"></param>
	/// <param name="key"></param>
	/// <exception cref="ConfigurationException">Configuration value is null or empty or whitespace</exception>
	/// <returns></returns>
	public static string GetRequiredValue(this IConfiguration configuration, string key)
	{
		ArgumentNullException.ThrowIfNull(configuration);
		ArgumentNullException.ThrowIfNull(key);

		var value = configuration[key];
		return string.IsNullOrWhiteSpace(value)
			? ConfigurationException.Throw<string>($"Required configuration key '{key}' does not have a value")
			: value;
	}
}
