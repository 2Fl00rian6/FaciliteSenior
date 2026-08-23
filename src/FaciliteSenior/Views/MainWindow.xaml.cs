using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FaciliteSenior.ViewModels;

namespace FaciliteSenior.Views;

public partial class MainWindow : Window
{
    private bool _isForcedFullScreen;
    private DispatcherFrame? _modalFrame;
    private bool _modalResult;

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Activated += OnWindowActivated;
        StateChanged += OnWindowStateChanged;
    }

    /// <summary>
    /// Affiche une confirmation/un message dans une carte au centre de CETTE fenetre
    /// (pas une vraie fenetre Windows separee). Reste synchrone comme un ShowDialog classique :
    /// on utilise le meme mecanisme interne (DispatcherFrame) que WPF utilise pour ses fenetres modales.
    /// </summary>
    public bool ShowConfirmationOverlay(string title, string message, string confirmText, string cancelText, bool isSingleAction)
    {
        ModalTitleText.Text = title;
        ModalMessageText.Text = message;
        ModalCodePanel.Visibility = Visibility.Collapsed;
        ModalConfirmText.Text = confirmText;
        ModalCancelText.Text = cancelText;
        ModalCancelButton.Visibility = isSingleAction ? Visibility.Collapsed : Visibility.Visible;

        return RunModal(focusCodeBox: false);
    }

    /// <summary>Meme principe pour la saisie du code d'acces aidant.</summary>
    public string? ShowPinPromptOverlay(string title, string message)
    {
        ModalTitleText.Text = title;
        ModalMessageText.Text = message;
        ModalCodePanel.Visibility = Visibility.Visible;
        ModalCodeBox.Password = string.Empty;
        ModalConfirmText.Text = "Valider";
        ModalCancelText.Text = "Annuler";
        ModalCancelButton.Visibility = Visibility.Visible;

        var confirmed = RunModal(focusCodeBox: true);
        return confirmed ? ModalCodeBox.Password : null;
    }

    private bool RunModal(bool focusCodeBox)
    {
        ModalOverlay.Visibility = Visibility.Visible;
        _modalResult = false;

        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (focusCodeBox)
            {
                ModalCodeBox.Focus();
                Keyboard.Focus(ModalCodeBox);
            }
            else
            {
                ModalConfirmButton.Focus();
                Keyboard.Focus(ModalConfirmButton);
            }
        }), DispatcherPriority.Input);

        _modalFrame = new DispatcherFrame();
        Dispatcher.PushFrame(_modalFrame);
        _modalFrame = null;

        ModalOverlay.Visibility = Visibility.Collapsed;
        return _modalResult;
    }

    private void CloseModal(bool result)
    {
        _modalResult = result;

        if (_modalFrame is not null)
        {
            _modalFrame.Continue = false;
        }
    }

    private void OnModalConfirmClick(object sender, RoutedEventArgs e)
    {
        CloseModal(true);
    }

    private void OnModalCancelClick(object sender, RoutedEventArgs e)
    {
        CloseModal(false);
    }

    private void OnModalOverlayPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (ModalOverlay.Visibility != Visibility.Visible)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            CloseModal(true);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            CloseModal(ModalCancelButton.Visibility != Visibility.Visible);
            e.Handled = true;
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel previousViewModel)
        {
            previousViewModel.FullScreenRequested -= OnFullScreenRequested;
            previousViewModel.RequestCloseApplication -= OnRequestCloseApplication;
        }

        if (e.NewValue is MainViewModel currentViewModel)
        {
            currentViewModel.FullScreenRequested += OnFullScreenRequested;
            currentViewModel.RequestCloseApplication += OnRequestCloseApplication;
            ApplyFullScreenMode(currentViewModel.IsFullScreen);
        }
    }

    private void OnFullScreenRequested(object? sender, bool isFullScreen)
    {
        ApplyFullScreenMode(isFullScreen);
    }

    private void OnRequestCloseApplication(object? sender, EventArgs e)
    {
        Close();
    }

    private void ApplyFullScreenMode(bool isFullScreen)
    {
        _isForcedFullScreen = isFullScreen;

        if (isFullScreen)
        {
            Topmost = true;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Normal;
            WindowState = WindowState.Maximized;
            Activate();
            return;
        }

        Topmost = false;
        ResizeMode = ResizeMode.CanResize;
        WindowStyle = WindowStyle.SingleBorderWindow;
        WindowState = WindowState.Maximized;
    }

    private void OnWindowActivated(object? sender, EventArgs e)
    {
        if (_isForcedFullScreen)
        {
            Topmost = true;
            Activate();
        }
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (_isForcedFullScreen && WindowState != WindowState.Maximized)
        {
            WindowState = WindowState.Maximized;
        }
    }
}
