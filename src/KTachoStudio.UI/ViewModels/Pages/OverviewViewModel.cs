using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KTachoStudio.Core.Adapters;

namespace KTachoStudio.UI.ViewModels.Pages;

public partial class OverviewViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _controllerPartNumber = "-";

    [ObservableProperty]
    private string _controllerComponent = "Not Connected";

    [ObservableProperty]
    private string _controllerCoding = "-";

    [ObservableProperty]
    private string _controllerWsc = "-";

    [ObservableProperty]
    private string _controllerVin = "-";

    [ObservableProperty]
    private string _clusterPin = "-";

    [ObservableProperty]
    private string _clusterMileage = "-";

    [ObservableProperty]
    private string _immoGeneration = "-";

    [ObservableProperty]
    private string _securityAccessStatus = "Locked";

    [ObservableProperty]
    private bool _isBusy = false;

    public void UpdateFromEcu(string partNo, string component, string coding, string wsc)
    {
        ControllerPartNumber = partNo;
        ControllerComponent = component;
        ControllerCoding = coding;
        ControllerWsc = wsc;
        SecurityAccessStatus = "Active / Diagnostic Session Open";
    }

    public void Reset()
    {
        ControllerPartNumber = "-";
        ControllerComponent = "Not Connected";
        ControllerCoding = "-";
        ControllerWsc = "-";
        ControllerVin = "-";
        ClusterPin = "-";
        ClusterMileage = "-";
        ImmoGeneration = "-";
        SecurityAccessStatus = "Locked";
    }

    [RelayCommand]
    private async Task ReadPinAsync()
    {
        IsBusy = true;
        try
        {
            var result = await Kw1281TesterWrapper.ReadClusterSkcAsync("MOCK_SIMULATOR", 10400);
            if (result.Success && !string.IsNullOrEmpty(result.Skc))
            {
                ClusterPin = result.Skc;
                ImmoGeneration = "Immobilizer 3 (Decoded via kw1281test)";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
