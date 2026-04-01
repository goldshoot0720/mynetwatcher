using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace NetWatcher.App;

public partial class App : Application
{
    private TrayIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private IClassicDesktopStyleApplicationLifetime? _desktop;
    private bool _isExitRequested;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktop = desktop;
            _desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
                Icon = AppIconFactory.CreateWindowIcon()
            };

            _mainWindow.Closing += MainWindowOnClosing;
            _mainWindow.PropertyChanged += MainWindowOnPropertyChanged;
            desktop.MainWindow = _mainWindow;
            InitializeTrayIcon();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitializeTrayIcon()
    {
        var showItem = new NativeMenuItem("顯示理由選單");
        showItem.Click += (_, _) => ShowMainWindow();

        var exitItem = new NativeMenuItem("結束程式");
        exitItem.Click += (_, _) => ExitApplication();

        var menu = new NativeMenu();
        menu.Add(showItem);
        menu.Add(exitItem);

        _trayIcon = new TrayIcon
        {
            Icon = AppIconFactory.CreateWindowIcon(),
            ToolTipText = "最瞎結婚理由選單",
            Menu = menu,
            IsVisible = true
        };
        _trayIcon.Clicked += (_, _) => ShowMainWindow();
    }

    private void MainWindowOnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_isExitRequested)
        {
            return;
        }

        e.Cancel = true;
        HideToTray();
    }

    private void MainWindowOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is not Window window || e.Property != Window.WindowStateProperty)
        {
            return;
        }

        if (window.WindowState == WindowState.Minimized)
        {
            HideToTray();
        }
    }

    private void HideToTray()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.ShowInTaskbar = false;
        _mainWindow.Hide();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.ShowInTaskbar = true;
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        _isExitRequested = true;

        if (_trayIcon is not null)
        {
            _trayIcon.IsVisible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        if (_mainWindow?.DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _mainWindow?.Close();
        _desktop?.Shutdown();
    }
}
