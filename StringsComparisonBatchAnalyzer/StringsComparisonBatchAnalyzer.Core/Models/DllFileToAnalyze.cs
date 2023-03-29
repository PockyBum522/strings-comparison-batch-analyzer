using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace StringsComparisonBatchAnalyzer.Core.Models;

/// <summary>
/// Helpful properties of a DLL to analyze
/// </summary>
public class DllFileToAnalyze
{
    /// <summary>
    /// The full path to the DLL
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// Filename only of the DLL
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// SHA-256 hash of the DLL
    /// </summary>
    public string Sha256Hash => GetHash();

    /// <summary>
    /// The results of the strings64 run. Will be empty until task completes.
    /// </summary>
    public string Strings64Results { get; set; } = "";
    
    /// <summary>
    /// Task that is running strings64.exe in a shell on the DLL
    /// </summary>
    public Task<string> RunStrings64OnFileTask { get; set; } 

    private string GetHash()
    {
        using var sha256 = SHA256.Create();
        using var fileStream = File.OpenRead(FilePath);
        
        var hashBytes = sha256.ComputeHash(fileStream);
        
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}