using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Serilog;
using StringsComparisonBatchAnalyzer.Core;
using StringsComparisonBatchAnalyzer.Core.Logic;
using StringsComparisonBatchAnalyzer.Core.Models.Configuration;

namespace StringsComparisonBatchAnalyzer.UI.WindowResources.MainWindow;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _textRunningAsUsernameMessage = "";
    [ObservableProperty] private string _currentFolderPathForDllsToAnalyze = "";
    [ObservableProperty] private string _reportsOutputFolderPath = "";
    
    [ObservableProperty] private string _applicationStatusLog = "";
    
    private readonly ILogger _logger;
    private readonly ISettingsApplicationLocal _settingsApplicationLocal;
    private readonly DllAnalyzerEngine _dllAnalyzerEngine;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    /// <param name="settingsApplicationLocal">Injected local application settings from disk</param>
    /// <param name="uiThreadDispatcher">UI Thread dispatcher so we can update WPF controls on the right thread</param>
    public MainWindowViewModel(
        ILogger logger,
        ISettingsApplicationLocal settingsApplicationLocal,
        Dispatcher uiThreadDispatcher)
    {
        _logger = logger;
        _settingsApplicationLocal = settingsApplicationLocal;

        _dllAnalyzerEngine = new DllAnalyzerEngine(logger, this, uiThreadDispatcher);
        
        ReportsOutputFolderPath = _settingsApplicationLocal.LastSelectedOutputPath;
        
        // Testing pre-fill
        if (Environment.UserName.ToLower().Contains("david"))
            CurrentFolderPathForDllsToAnalyze = @"D:\Dropbox\Documents\Desktop\Dlls n Shit\";
    }

    /// <summary>
    /// Adds a message to the status log textbox that is shown to the user
    /// </summary>
    /// <param name="message">Message to add to the status log textbox</param>
    /// <param name="newLinesFollowing">Optional, defaults to 2. New lines </param>
    public void AppendToStatusLog(string message, int newLinesFollowing = 2)
    {
        ApplicationStatusLog += message;

        for (var i = 0; i < newLinesFollowing; i++)
        {
            ApplicationStatusLog += Environment.NewLine;    
        }
    }
    
    /// <summary>
    /// Prompts the user to browse for the folder containing the DLLs to analyze
    /// </summary>
    [RelayCommand]
    private void PromptUserForDllsFolder()
    {
        CurrentFolderPathForDllsToAnalyze = PromptUserForFolder();
    }
    
    /// <summary>
    /// Prompts the user to browse for the folder containing the DLLs to analyze
    /// </summary>
    [RelayCommand]
    private void PromptUserForOutputFolder()
    {
        ReportsOutputFolderPath = PromptUserForFolder();
    }

    /// <summary>
    /// Prompts the user to browse to a folder, then returns that path
    /// </summary>
    private string PromptUserForFolder()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);

        var desktopDirectoryPath = 
            Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
                .Replace("/", @"\");

        Console.WriteLine(desktopDirectoryPath);

        var folderSelectionPlaceholder = "Folder Selection";
        
        var folderPickerDialog = new OpenFileDialog()
        {
            InitialDirectory = desktopDirectoryPath,
            ValidateNames = false,
            CheckFileExists = false,
            FileName = folderSelectionPlaceholder
        };

        if (folderPickerDialog.ShowDialog() != true) return "";

        var fullSelectedFolderPath = folderPickerDialog.FileName;

        if (fullSelectedFolderPath.EndsWith(folderSelectionPlaceholder))
            fullSelectedFolderPath = fullSelectedFolderPath.Replace(folderSelectionPlaceholder, "");
        
        return fullSelectedFolderPath;
    }


    /// <summary>
    /// Prompts the user to browse for the folder containing the DLLs to analyze
    /// </summary>
    [RelayCommand]
    private async Task RunBatchOnFolder()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);
        _logger.Information("Selected folder path is: {CurrentFolderPath}", CurrentFolderPathForDllsToAnalyze);
        
        if (string.IsNullOrWhiteSpace(CurrentFolderPathForDllsToAnalyze) ||
            string.IsNullOrWhiteSpace(ReportsOutputFolderPath))
        {
            MessageBox.Show("No folder selected for DLLs folder or reports output path. Try again");
            
            return;
        }

        _settingsApplicationLocal.LastSelectedOutputPath = ReportsOutputFolderPath;
        
        // Otherwise:
        if (Directory.GetFiles(CurrentFolderPathForDllsToAnalyze, "*.dll").Length < 1)
        {
            MessageBox.Show(
                $"""
                No dll files found directly in directory: {CurrentFolderPathForDllsToAnalyze}

                Please pick another
                """);
                
            return;
        }
        
        // Otherwise:
        _logger.Debug("Passed validation checks on selected folder, starting to analyze dlls");
        
        // Start analyzing 
        await _dllAnalyzerEngine.GenerateDllFileReports(CurrentFolderPathForDllsToAnalyze, ReportsOutputFolderPath);
    }
}