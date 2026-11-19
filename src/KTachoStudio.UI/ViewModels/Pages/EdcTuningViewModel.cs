using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KTachoStudio.Core.Adapters;

namespace KTachoStudio.UI.ViewModels.Pages;

public partial class EdcTuningViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _ecuModel = "Bosch EDC15P+ (C167 + 29F400BT)";

    [ObservableProperty]
    private string _flashFilePath = string.Empty;

    [ObservableProperty]
    private string _flashFileSize = "512 KB (16 Sectors)";

    [ObservableProperty]
    private bool _smartSectorCheck = true;

    [ObservableProperty]
    private bool _verifyChecksum = true;

    [ObservableProperty]
    private bool _fastInitEnabled = false;

    [ObservableProperty]
    private int _flashProgressPercent = 0;

    [ObservableProperty]
    private string _flashOperationStatus = "Ready";

    [ObservableProperty]
    private bool _isBusy = false;

    [RelayCommand]
    private async Task DumpFlashAsync()
    {
        if (string.IsNullOrWhiteSpace(FlashFilePath))
        {
            FlashFilePath = "EDC15_Flash.bin";
        }

        IsBusy = true;
        FlashProgressPercent = 0;
        FlashOperationStatus = "Starting EDC15 Flash Read via kw1281test...";

        try
        {
            var success = await Kw1281TesterWrapper.DumpEdc15FlashAsync(
                "MOCK_SIMULATOR", 10400, FlashFilePath, p => FlashProgressPercent = p);

            FlashOperationStatus = success ? $"Flash dump saved to {FlashFilePath}" : "Flash dump finished.";
            FlashProgressPercent = 100;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BootRecoveryAsync()
    {
        IsBusy = true;
        FlashOperationStatus = "Entering EDC15 C167 Boot Recovery Mode via kw1281test...";
        await Task.Delay(100);
        IsBusy = false;
    }

    [RelayCommand]
    private async Task WriteFlashAsync()
    {
        if (string.IsNullOrWhiteSpace(FlashFilePath))
        {
            FlashOperationStatus = "Select a .bin file before writing.";
            return;
        }

        IsBusy = true;
        FlashProgressPercent = 0;
        FlashOperationStatus = "Writing EDC15 Flash via kw1281test...";

        try
        {
            var success = await Kw1281TesterWrapper.LoadEdc15FlashAsync(
                "MOCK_SIMULATOR", 10400, FlashFilePath, p => FlashProgressPercent = p);

            FlashOperationStatus = success ? "Flash write complete and verified." : "Flash write failed.";
            FlashProgressPercent = 100;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
