using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KTachoStudio.UI.ViewModels.Pages;

public partial class ComfortModulesViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _windowRollUpEnabled = true;

    [ObservableProperty]
    private bool _alarmChirpEnabled = true;

    [ObservableProperty]
    private bool _autoLockEnabled = true;

    [ObservableProperty]
    private bool _selectiveUnlock = false;

    [ObservableProperty]
    private string _radioSafeCode = "1984";

    [RelayCommand]
    private void ReadCcm()
    {
    }

    [RelayCommand]
    private void ApplyCcm()
    {
    }

    [RelayCommand]
    private void ClearAirbagCrash()
    {
    }

    [RelayCommand]
    private void RetrieveRadioCode()
    {
    }
}
