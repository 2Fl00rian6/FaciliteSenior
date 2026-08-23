using System.Windows;
using FaciliteSenior.Views;

namespace FaciliteSenior.Services;

/// <summary>
/// Toutes les boites de dialogue s'affichent DANS la fenetre principale (voile + carte centree),
/// jamais comme de vraies fenetres Windows separees, pour rester coherent avec l'esprit
/// "une seule application" attendu par un utilisateur senior.
/// </summary>
public sealed class DialogService : IDialogService
{
    public bool Confirm(string title, string message, string confirmText = "Oui", string cancelText = "Annuler")
    {
        return GetMainWindow().ShowConfirmationOverlay(title, message, confirmText, cancelText, isSingleAction: false);
    }

    public void ShowMessage(string title, string message, string closeText = "Fermer")
    {
        GetMainWindow().ShowConfirmationOverlay(title, message, closeText, "Fermer", isSingleAction: true);
    }

    public bool RequestAccessCode(string expectedCode, string title, string message)
    {
        if (string.IsNullOrWhiteSpace(expectedCode))
        {
            return true;
        }

        var enteredCode = GetMainWindow().ShowPinPromptOverlay(title, message);
        return enteredCode is not null && string.Equals(enteredCode.Trim(), expectedCode.Trim(), StringComparison.Ordinal);
    }

    private static MainWindow GetMainWindow()
    {
        return (MainWindow)Application.Current.MainWindow!;
    }
}
