namespace StringsComparisonBatchAnalyzer.Core.Models.Configuration;

/// <summary>
/// Proxy for the configuration settings for the application, handled by Config.net library
/// </summary>
public interface ISettingsApplicationLocal
{
    /// <summary>
    /// The last output (reports) path that the user selected
    /// </summary>
    public string LastSelectedOutputPath { get; set; }
}