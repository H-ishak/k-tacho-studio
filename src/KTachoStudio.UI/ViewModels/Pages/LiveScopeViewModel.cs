using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KTachoStudio.UI.ViewModels.Pages;

public partial class LiveScopeViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _engineRpm = "1280";

    [ObservableProperty]
    private string _boostActual = "1980 mbar";

    [ObservableProperty]
    private string _mafActual = "850 mg/str";

    [ObservableProperty]
    private string _injectionQuantity = "42.5 mg/str";

    [ObservableProperty]
    private string _coolantTemp = "90 °C";

    [ObservableProperty]
    private int _selectedGroup = 3;

    [RelayCommand]
    private void ScanDtcs()
    {
    }

    [RelayCommand]
    private void ClearDtcs()
    {
    }

    [RelayCommand]
    private void StartActuatorTest()
    {
    }
}
