namespace StringsComparisonBatchAnalyzer.Core.Models;

public class OverlapInstance
{
    public string FileNameOne { get; set; } = "";
    public string FileNameTwo { get; set; } = "";

    public string MatchedStringOne { get; set; } = "";
    public string MatchedStringTwo { get; set; } = "";

    public override string ToString()
    {
        return $"Match in {FileNameOne} / {FileNameTwo} - {MatchedStringOne} | {MatchedStringTwo}";
    }
}