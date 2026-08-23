using System.Windows;
using System.Windows.Controls;
using FaciliteSenior.ViewModels;

namespace FaciliteSenior.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
        Loaded += OnLayoutChanged;
        SizeChanged += OnLayoutChanged;
        DataContextChanged += OnLayoutChanged;
    }

    private void OnLayoutChanged(object? sender, RoutedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void OnLayoutChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        if (DataContext is not HomeViewModel viewModel)
        {
            return;
        }

        var availableWidth = Math.Max(ActualWidth, 320);
        var compact = availableWidth < 760;
        var targetCardWidth = compact ? 240 * viewModel.CardScale : 320 * viewModel.CardScale;
        var columns = Math.Max(1, (int)Math.Floor((availableWidth - 24) / Math.Max(180, targetCardWidth)));

        viewModel.IsCompactLayout = compact;
        viewModel.Columns = Math.Min(columns, compact ? 2 : 4);
    }
}
