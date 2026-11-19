using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KTachoStudio.Core.Adapters;

namespace KTachoStudio.UI.ViewModels.Pages;

public partial class ClusterImmoViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _clusterType = "-";

    [ObservableProperty]
    private string _clusterPin = "-";

    [ObservableProperty]
    private string _targetClusterPin = string.Empty;

    [ObservableProperty]
    private string _clusterMileage = "-";

    [ObservableProperty]
    private string _immoGeneration = "-";

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _operationStatus = "Ready";

    [RelayCommand]
    private async Task ReadPinAsync()
    {
        IsBusy = true;
        OperationStatus = "Reading SKC/PIN via kw1281test...";
        try
        {
            var (success, skc, _) = await Kw1281TesterWrapper.ReadClusterSkcAsync("MOCK_SIMULATOR", 10400);
            if (success)
            {
                ClusterPin = skc;
                ClusterType = "VDO V08 (Golf Mk4)";
                ImmoGeneration = "Immobilizer 3";
                OperationStatus = $"Success: Extracted PIN {skc}";
            }
            else
            {
                OperationStatus = "Failed to extract PIN from cluster.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DumpEepromAsync()
    {
        IsBusy = true;
        OperationStatus = "Dumping cluster EEPROM via kw1281test...";
        try
        {
            var filename = "Cluster_Dump.bin";
            var bytes = await Kw1281TesterWrapper.DumpEdc15EepromAsync("MOCK_SIMULATOR", 10400, filename);
            OperationStatus = bytes != null ? $"Dumped EEPROM to {filename}" : "EEPROM dump completed.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task WriteEepromAsync()
    {
        IsBusy = true;
        OperationStatus = "Writing cluster EEPROM via kw1281test...";
        await Task.Delay(100);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task StartAdaptationAsync()
    {
        IsBusy = true;
        OperationStatus = "Starting Cluster / Immobilizer key adaptation...";
        await Task.Delay(100);
        IsBusy = false;
    }
}
