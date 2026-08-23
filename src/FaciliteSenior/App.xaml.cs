using System.Windows;
using FaciliteSenior.Services;
using FaciliteSenior.ViewModels;
using FaciliteSenior.Views;

namespace FaciliteSenior;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        var configService = new ConfigService();
        var dialogService = new DialogService();
        var startupService = new WindowsStartupService();
        var mainViewModel = new MainViewModel(configService, dialogService, startupService);
        var mainWindow = new MainWindow
        {
            DataContext = mainViewModel
        };

        MainWindow = mainWindow;

        await mainViewModel.InitializeAsync();
        mainWindow.Show();
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            "L'application a rencontre un probleme inattendu. Revenez a l'accueil puis reessayez.",
            "Probleme detecte",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        e.Handled = true;
    }
}
