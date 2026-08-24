using FaciliteSenior.Models;

namespace FaciliteSenior.ViewModels;

public sealed class HelpViewModel : ViewModelBase
{
    private string _title = "Besoin d'aide ?";
    private string _message = "En cas de probleme, appelle ton aidant.";
    private string _helperName = "Votre aidant";
    private string _phoneNumber = "00 00 00 00 00";

    public string Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public string HelperName
    {
        get => _helperName;
        private set => SetProperty(ref _helperName, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        private set => SetProperty(ref _phoneNumber, value);
    }

    public string CallSummary => $"En cas de probleme, appelle {HelperName} au {PhoneNumber}.";

    public void Load(HelpContent help)
    {
        Title = help.Title;
        Message = help.Message;
        HelperName = help.HelperName;
        PhoneNumber = help.PhoneNumber;
        OnPropertyChanged(nameof(CallSummary));
    }
}
