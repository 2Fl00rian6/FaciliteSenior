using System.Windows;
using System.Windows.Controls;
using FaciliteSenior.ViewModels;

namespace FaciliteSenior.Views;

public partial class DocumentsView : UserControl
{
    public DocumentsView()
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
        if (DataContext is not DocumentsViewModel viewModel)
        {
            return;
        }

        var availableWidth = Math.Max(ActualWidth, 320);
        var compact = availableWidth < 760;
        var targetCardWidth = compact ? 220 * viewModel.CardScale : 300 * viewModel.CardScale;
        var columns = Math.Max(1, (int)Math.Floor((availableWidth - 24) / Math.Max(170, targetCardWidth)));

        viewModel.IsCompactLayout = compact;
        viewModel.Columns = Math.Min(columns, compact ? 2 : 4);
    }
}
