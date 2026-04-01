using Avalonia.Controls;

namespace NetWatcher.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void CopyReasonButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
        {
            viewModel.SetCopyStatus("目前無法使用剪貼簿。");
            return;
        }

        await clipboard.SetTextAsync(viewModel.ShareText);
        viewModel.SetCopyStatus("已複製到剪貼簿，現場直接朗讀也可以。");
    }
}
