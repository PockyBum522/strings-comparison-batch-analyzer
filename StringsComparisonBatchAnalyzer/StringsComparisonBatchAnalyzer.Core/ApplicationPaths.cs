using System;
using System.IO;

namespace StringsComparisonBatchAnalyzer.Core
{
    /// <summary>
    /// Contains the few paths for this application that must be hardcoded
    /// </summary>
    public static class ApplicationPaths
    {
        private static string ApplicationBaseDataPath =>
            Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Strings Comparison DLLs Analyzer");
        
        /// <summary>
        /// Per-user log folder path
        /// </summary>
        private static string LogAppBasePath =>
            Path.Combine(
                ApplicationBaseDataPath,
                "Logs");

        /// <summary>
        /// Actual log file path passed to the ILogger configuration
        /// </summary>
        public static string LogPath =>
            Path.Combine(
                LogAppBasePath,
                "NetworkManager.log");
        
        /// <summary>
        /// The directory the assembly is running from
        /// </summary>
        public static string ApplicationRunFromDirectoryPath => 
            Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        
        /// <summary>
        /// The full path to this application's running assembly
        /// </summary>
        public static string ApplicationProcessPath => 
            Environment.ProcessPath ?? "";

        /// <summary>
        /// The full path to the dark theme Styles.xaml which contains the rest of the style information
        /// </summary>
        public static string DarkThemePath =>
                Path.Join(
                    ApplicationRunFromDirectoryPath,
                    "Themes",
                    "SelenMetroDark",
                    "Styles.xaml");

        /// <summary>
        /// Path to the .ini file handled by Config.net library to save application data
        /// </summary>
        public static string PathSettingsApplicationLocalIniFile =>
            Path.Join(ApplicationBaseDataPath,
                "Settings",
                "saved_user_settings.ini");
    }
}
