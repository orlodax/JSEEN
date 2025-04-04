using Microsoft.UI.Xaml;

namespace JSEEN;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class. /ᐠ｡ꞈ｡ᐟ\
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        MainWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
        m_window.Activate();
    }

    private Window? m_window;

    public static nint MainWindowHandle { get; private set; }
}
