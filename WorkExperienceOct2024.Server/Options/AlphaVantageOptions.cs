namespace WorkExperienceOct2024.Server.Options;

public sealed class AlphaVantageOptions
{
    /// <summary>
    /// Alpha Vantage API key used to authenticate requests. Configure via
    /// configuration (AlphaVantage:ApiKey) or the ALPHAVANTAGE_API_KEY environment variable.
    /// </summary>
    public string? ApiKey { get; set; }
}
