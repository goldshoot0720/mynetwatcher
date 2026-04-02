using Avalonia;
using Avalonia.Controls;

namespace NetWatcher.App;

public partial class MainWindow : Window
{
    private const double WideLayoutBreakpoint = 1380;

    public MainWindow()
    {
        InitializeComponent();
        Opened += (_, _) => UpdateResponsiveLayout();
        SizeChanged += MainWindow_OnSizeChanged;
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

    private void MainWindow_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateResponsiveLayout();
    }

    private void UpdateResponsiveLayout()
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.UpdateLayoutForWidth(Bounds.Width);
        }

        var isWideLayout = Bounds.Width >= WideLayoutBreakpoint;

        HeroLayoutGrid.ColumnDefinitions = ColumnDefinitions.Parse(isWideLayout ? "1.35*,1*" : "*");
        HeroLayoutGrid.RowDefinitions = RowDefinitions.Parse(isWideLayout ? "*" : "Auto,Auto");
        Grid.SetRow(SummarySection, 0);
        Grid.SetColumn(SummarySection, 0);
        Grid.SetRow(HistorySection, isWideLayout ? 0 : 1);
        Grid.SetColumn(HistorySection, isWideLayout ? 1 : 0);

        TitleGrid.ColumnDefinitions = ColumnDefinitions.Parse(isWideLayout ? "*,Auto" : "*");
        Grid.SetRow(LastUpdatedBadge, isWideLayout ? 0 : 1);
        Grid.SetColumn(LastUpdatedBadge, isWideLayout ? 1 : 0);
        LastUpdatedBadge.Margin = isWideLayout ? new Thickness(0) : new Thickness(0, 12, 0, 0);

        OverviewMetricsPanel.ItemWidth = isWideLayout ? 280 : 250;
        OverviewMetricsPanel.ItemHeight = isWideLayout ? 108 : 100;
        DownloadMetricCard.Width = isWideLayout ? 280 : 250;
        UploadMetricCard.Width = isWideLayout ? 280 : 250;

        HistoryHeaderGrid.ColumnDefinitions = ColumnDefinitions.Parse(isWideLayout ? "*,Auto" : "*");
        Grid.SetRow(HistoryScaleBadge, isWideLayout ? 0 : 1);
        Grid.SetColumn(HistoryScaleBadge, isWideLayout ? 1 : 0);
        HistoryScaleBadge.Margin = isWideLayout ? new Thickness(0) : new Thickness(0, 10, 0, 0);

        ExportGrid.ColumnDefinitions = ColumnDefinitions.Parse(isWideLayout ? "Auto,*,Auto" : "*");
        Grid.SetRow(ExportStatusPanel, isWideLayout ? 0 : 1);
        Grid.SetColumn(ExportStatusPanel, isWideLayout ? 1 : 0);
        Grid.SetRow(ExportHintText, isWideLayout ? 0 : 2);
        Grid.SetColumn(ExportHintText, 0);
        ExportHintText.Margin = isWideLayout ? new Thickness(0) : new Thickness(0, 2, 0, 0);

        SectionHeaderGrid.ColumnDefinitions = ColumnDefinitions.Parse(isWideLayout ? "*,Auto" : "*");
        Grid.SetRow(SectionMetaText, isWideLayout ? 0 : 1);
        Grid.SetColumn(SectionMetaText, isWideLayout ? 1 : 0);
        SectionMetaText.Margin = isWideLayout ? new Thickness(0) : new Thickness(0, 6, 0, 0);

        ProcessHeaderGrid.ColumnDefinitions = ColumnDefinitions.Parse(isWideLayout ? "*,Auto" : "*");
        Grid.SetRow(ProcessToggleButton, isWideLayout ? 0 : 1);
        Grid.SetColumn(ProcessToggleButton, isWideLayout ? 1 : 0);
        ProcessToggleButton.Margin = isWideLayout ? new Thickness(0) : new Thickness(0, 10, 0, 0);

        SearchTextBox.Width = isWideLayout ? 360 : 320;
    }
}
