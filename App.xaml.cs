using System.Configuration;
using System.Data;
using System.Windows;
using System.Diagnostics;
using System.Threading;

namespace SkyboundLauncher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private Services.LoggingService? _loggingService;
    private static Mutex? _instanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Check for multiple instances - kill previous if running
        EnsureSingleInstance();

        // Initialize Debug Output Listener
        Trace.Listeners.Add(new DebugOutputListener());

        // Initialize logging
        _loggingService = new Services.LoggingService();
        _loggingService.LogInfo("=== Skybound Launcher started ===");

        // Global error handler
        this.DispatcherUnhandledException += (s, ex) =>
        {
            _loggingService.LogError("Unhandled exception", ex.Exception);
            MessageBox.Show($"An unexpected error occurred:\n{ex.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
        {
            _loggingService?.LogError("Domain UnhandledException", ex.ExceptionObject as Exception);
        };
    }

    /// <summary>
    /// Ensures only one instance of the launcher is running.
    /// If another instance is already running, it will be terminated.
    /// </summary>
    private void EnsureSingleInstance()
    {
        try
        {
            const string mutexName = "SkyboundLauncher_SingleInstance";
            
            // Try to create or open the mutex
            _instanceMutex = new Mutex(true, mutexName, out bool createdNew);
            
            if (!createdNew)
            {
                // Mutex already exists, meaning another instance is running
                // Kill all other instances of this application
                var currentProcess = Process.GetCurrentProcess();
                var otherProcesses = Process.GetProcessesByName(currentProcess.ProcessName);
                
                foreach (var process in otherProcesses)
                {
                    // Don't kill the current process
                    if (process.Id != currentProcess.Id)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit(5000); // Wait up to 5 seconds for process to exit
                            Debug.WriteLine($"[App] Killed previous launcher instance (PID: {process.Id})");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[App] Error killing process: {ex.Message}");
                        }
                    }
                }
                
                // Now acquire the mutex for this instance
                _instanceMutex.Dispose();
                _instanceMutex = new Mutex(true, mutexName);
            }
            
            Debug.WriteLine("[App] Single instance check passed - launcher is running exclusively");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] Error in EnsureSingleInstance: {ex.Message}");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _loggingService?.LogInfo("=== Skybound Launcher closed ===");
        
        // Release the mutex when exiting
        _instanceMutex?.ReleaseMutex();
        _instanceMutex?.Dispose();
        
        base.OnExit(e);
    }
}
