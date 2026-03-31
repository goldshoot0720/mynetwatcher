using Avalonia.Controls;

namespace NetWatcher.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ExportCsvButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.ExportTrafficHistoryAsync();
        }
    }

    private void ToggleProcessSectionButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.IsProcessSectionExpanded = !viewModel.IsProcessSectionExpanded;
        }
    }
}
