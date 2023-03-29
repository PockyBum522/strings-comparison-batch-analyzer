using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace StringsComparisonBatchAnalyzer.UI.WindowResources.MainWindow;

/// <summary>
/// The ViewModel for MainWindow
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _textRunningAsUsernameMessage = "";
    
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="logger">Injected ILogger to use</param>
    public MainWindowViewModel(
        ILogger logger)
    {
        _logger = logger;
        
    }

    [RelayCommand]
    private Task StartMainSetupProcess()
    {
        _logger.Information("Running {ThisName}", System.Reflection.MethodBase.GetCurrentMethod()?.Name);

        MessageBox.Show("This is a working thing. Wooooo.");
        
        return Task.CompletedTask;
    }
    
}