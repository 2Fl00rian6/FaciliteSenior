namespace FaciliteSenior.Services;

public interface IDialogService
{
    bool Confirm(string title, string message, string confirmText = "Oui", string cancelText = "Annuler");

    void ShowMessage(string title, string message, string closeText = "Fermer");

    bool RequestAccessCode(string expectedCode, string title, string message);
}
