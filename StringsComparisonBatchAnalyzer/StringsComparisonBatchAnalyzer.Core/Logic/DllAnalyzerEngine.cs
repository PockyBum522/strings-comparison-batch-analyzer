using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Serilog;
using SevenZipExtractor;
using StringsComparisonBatchAnalyzer.Core.Models;
using StringsComparisonBatchAnalyzer.UI.WindowResources.MainWindow;

namespace StringsComparisonBatchAnalyzer.Core.Logic;

/// <summary>
/// Handles analyzing all DLLs in selected folder
/// </summary>
public class DllAnalyzerEngine
{
    private readonly ILogger _logger;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly Dispatcher _uiThreadDispatcher;

    private string _currentFolderPath = "";
    private string _reportsOutputPath = "";

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    /// <param name="mainWindowViewModel">MainWindowViewModel so we can update the status log the user sees</param>
    /// <param name="uiThreadDispatcher"></param>
    public DllAnalyzerEngine(ILogger logger,
        MainWindowViewModel mainWindowViewModel, 
        Dispatcher uiThreadDispatcher)
    {
        _logger = logger;
        _mainWindowViewModel = mainWindowViewModel;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    /// <summary>
    /// Grabs all DLLs in passed folder path, analyzes them, generates reports
    /// </summary>
    /// <param name="currentFolderPath">Folder with DLLs to analyze</param>
    /// <param name="reportsOutputFolderPath">Where to put the reports</param>
    public async Task GenerateDllFileReports(string currentFolderPath, string reportsOutputFolderPath)
    {
        _reportsOutputPath = reportsOutputFolderPath;
        _currentFolderPath = currentFolderPath;
        
        MakeReportsOutputFolderOnDesktop();

        var listOfDllPaths = Directory.GetFiles(_currentFolderPath, "*.dll").ToList();

        var initializedDllFilesToAnalyze = InitializeAllDlls(listOfDllPaths);

        var dllFilesToAnalyzeWithStrings64Data = await MakeStringsReportForAll(initializedDllFilesToAnalyze);

        var dllFilesToAnalyzeWithAllReports = await MakeDetectItEasyReportForAll(dllFilesToAnalyzeWithStrings64Data);
        
        Console.WriteLine(dllFilesToAnalyzeWithAllReports.Count);
    }

    private async Task<List<DllFileToAnalyze>> MakeStringsReportForAll(List<DllFileToAnalyze> dllsToAnalyze)
    {
        var dllsStringsTasks = new Task[dllsToAnalyze.Count];
        
        var i = 0;
        
        foreach (var dllToAnalyze in dllsToAnalyze)
        {
            var temporaryCurrentDllWorkPath =
                Path.Join(_reportsOutputPath, "Temp", "Strings64 Outputs", dllToAnalyze.FileName.Replace(".", "_"));

            Directory.CreateDirectory(temporaryCurrentDllWorkPath);
            
            dllsStringsTasks[i++] = dllToAnalyze.RunStrings64OnFileTask;
        }

        var allTasksComplete = false;
        
        while (!allTasksComplete)
        {
            allTasksComplete = true;
            
            var stringOfTasksNotYetFinished = "";
            
            foreach (var dllWeAreWaitingOn in dllsToAnalyze)
            {
                if (dllWeAreWaitingOn.RunStrings64OnFileTask.IsCompleted) continue;
                
                allTasksComplete = false;
                stringOfTasksNotYetFinished += $"{dllWeAreWaitingOn.FileName}, ";
            }
            
            _uiThreadDispatcher.BeginInvoke(() => 
                _mainWindowViewModel.AppendToStatusLog(
                    $"Waiting on the following to finish strings64.exe report: {Environment.NewLine}{stringOfTasksNotYetFinished}"));
            
            await Task.Delay(1000);
        }

        foreach (var dllToAwait in dllsToAnalyze)
        {
            dllToAwait.Strings64Results = await dllToAwait.RunStrings64OnFileTask;
        }

        return dllsToAnalyze;
    }
    
    private async Task<List<DllFileToAnalyze>> MakeDetectItEasyReportForAll(List<DllFileToAnalyze> dllsToAnalyze)
    {
        ExtractDetectItEasy();
        
        var dllsStringsTasks = new Task[dllsToAnalyze.Count];
        
        var i = 0;
        
        foreach (var dllToAnalyze in dllsToAnalyze)
        {
            var temporaryCurrentDllWorkPath =
                Path.Join(_reportsOutputPath, "Temp", "Strings64 Outputs", dllToAnalyze.FileName.Replace(".", "_"));

            Directory.CreateDirectory(temporaryCurrentDllWorkPath);
            
            dllsStringsTasks[i++] = dllToAnalyze.RunStrings64OnFileTask;
        }

        var allTasksComplete = false;
        
        while (!allTasksComplete)
        {
            allTasksComplete = true;
            
            var stringOfTasksNotYetFinished = "";
            
            foreach (var dllWeAreWaitingOn in dllsToAnalyze)
            {
                if (dllWeAreWaitingOn.RunStrings64OnFileTask.IsCompleted) continue;
                
                allTasksComplete = false;
                stringOfTasksNotYetFinished += $"{dllWeAreWaitingOn.FileName}, ";
            }
            
            _uiThreadDispatcher.BeginInvoke(() => 
                _mainWindowViewModel.AppendToStatusLog(
                    $"Waiting on the following to finish strings64.exe report: {Environment.NewLine}{stringOfTasksNotYetFinished}"));
            
            await Task.Delay(1000);
        }

        foreach (var dllToAwait in dllsToAnalyze)
        {
            dllToAwait.Strings64Results = await dllToAwait.RunStrings64OnFileTask;
        }

        return dllsToAnalyze;
    }

    private void ExtractDetectItEasy()
    {
        var detectItEasyArchivePath = Path.Join(
            ApplicationPaths.ApplicationRunFromDirectoryPath, 
            "Resources",
            "Detect It Easy", 
            "Detect It Easy.7z");
        
        var outputPath = Path.Join(
            ApplicationPaths.ApplicationRunFromDirectoryPath, 
            "Resources",
            "Detect It Easy",
            "Extracted");

        Directory.CreateDirectory(outputPath);

        using var archiveFile = new ArchiveFile(detectItEasyArchivePath);
        
        archiveFile.Extract(outputPath); // extract all
    }

    private void MakeReportsOutputFolderOnDesktop()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);

        var newFolderBuiltPath = 
            Path.Join(
                    _reportsOutputPath,
                    DateTimeOffset.Now.ToString("yyyy-MM-dd, hh.mm.ss tt"))
                .Replace("/", @"\");

        _logger.Information("Creating folder at path: {FolderPath}", _reportsOutputPath);

        _reportsOutputPath = newFolderBuiltPath;
        
        Directory.CreateDirectory(_reportsOutputPath);
    }
    
    private List<DllFileToAnalyze> InitializeAllDlls(List<string> listOfDlls)
    {
        _logger.Information("Getting all DLLs in: {SelectedFolder}", _currentFolderPath);

        _uiThreadDispatcher.BeginInvoke(() => 
            _mainWindowViewModel.AppendToStatusLog($"Getting all DLLs in: {_currentFolderPath}"));

        var dllsToAnalyze = new List<DllFileToAnalyze>();
        
        foreach (var dllPath in listOfDlls)
        {
            dllsToAnalyze.Add(
                InitializeDll(dllPath));
        }

        return dllsToAnalyze;
    }

    private DllFileToAnalyze InitializeDll(string dllPath)
    {
        _logger.Information("Initializing analysis of DLL: {DllPath}", dllPath);

        var strings64ExePath = Path.Join(
            ApplicationPaths.ApplicationRunFromDirectoryPath, 
            "Resources",
            "Strings Sysinternals", 
            "strings64.exe");

        var strings64Arguments = $"""-nobanner "{dllPath}" """;

        var currentDll = new DllFileToAnalyze()
        {
            FilePath = dllPath,
            RunStrings64OnFileTask = RunProcessAsync(strings64ExePath, strings64Arguments)
        };
            
        _uiThreadDispatcher.BeginInvoke(() => 
            _mainWindowViewModel.AppendToStatusLog($"Starting strings64.exe report for: {currentDll.FileName}"));

        return currentDll;
    }
    
    private async Task<string> RunProcessAsync(string exePath, string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var output = new StringBuilder();
        
        using var process = new Process { StartInfo = processStartInfo };
        
        process.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);
        process.ErrorDataReceived += (sender, args) => output.AppendLine(args.Data);

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return output.ToString();
    }
}