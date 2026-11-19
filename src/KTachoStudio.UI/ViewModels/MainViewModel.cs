using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KTachoStudio.Core.Adapters;
using KTachoStudio.UI.ViewModels.Pages;

namespace KTachoStudio.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // -------------------------------------------------------------
    // Hardware Toolbar State (Global Header)
    // -------------------------------------------------------------
    [ObservableProperty]
    private string _selectedPort = "MOCK_SIMULATOR";

    [ObservableProperty]
    private ObservableCollection<string> _availablePorts = new() { "MOCK_SIMULATOR", "COM1", "COM3", "COM4", "/dev/ttyUSB0" };

    [ObservableProperty]
    private string _selectedBaud = "10400 (Standard)";

    [ObservableProperty]
    private ObservableCollection<string> _availableBauds = new() { "10400 (Standard)", "9600 (Marelli)", "19200", "38400", "57600 (High Speed)" };

    [ObservableProperty]
    private bool _isConnected = false;

    [ObservableProperty]
    private string _connectionStatusText = "CONNECT";

    [ObservableProperty]
    private bool _isDetecting = false;

    [ObservableProperty]
    private string _autoDetectButtonText = "AUTO-DETECT";

    [ObservableProperty]
    private string _batteryVoltage = "13.8V Bench";

    // -------------------------------------------------------------
    // MVC / Page Router & Current View
    // -------------------------------------------------------------
    public OverviewViewModel OverviewPage { get; } = new();
    public EdcTuningViewModel EdcTuningPage { get; } = new();
    public ClusterImmoViewModel ClusterImmoPage { get; } = new();
    public LiveScopeViewModel LiveScopePage { get; } = new();
    public ComfortModulesViewModel ComfortModulesPage { get; } = new();
    public HexLogViewModel HexLogPage { get; } = new();

    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    // -------------------------------------------------------------
    // Bottom Telemetry Strip (Global Footer)
    // -------------------------------------------------------------
    [ObservableProperty]
    private bool _txActive = false;

    [ObservableProperty]
    private bool _rxActive = false;

    [ObservableProperty]
    private string _bottomStatusMessage = "Ready. Select Port and click CONNECT (or use MOCK_SIMULATOR to test offline).";

    [ObservableProperty]
    private int _operationProgress = 0;

    [ObservableProperty]
    private string _operationProgressStatus = "Idle";

    public MainViewModel()
    {
        _currentPage = OverviewPage;

        // Auto-populate available COM ports
        try
        {
            foreach (var port in System.IO.Ports.SerialPort.GetPortNames())
            {
                if (!AvailablePorts.Contains(port))
                    AvailablePorts.Add(port);
            }
        }
        catch { }

        // Forward diagnostic messages to the bottom status strip
        Kw1281TesterWrapper.OnLogMessage += line =>
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    BottomStatusMessage = line;
                });
            }
        };
    }

    // -------------------------------------------------------------
    // Router Commands
    // -------------------------------------------------------------
    [RelayCommand]
    private void Navigate(string target)
    {
        if (!int.TryParse(target, out var index)) return;
        SelectedTabIndex = index;

        CurrentPage = index switch
        {
            0 => OverviewPage,
            1 => EdcTuningPage,
            2 => ClusterImmoPage,
            3 => LiveScopePage,
            4 => ComfortModulesPage,
            5 => HexLogPage,
            _ => OverviewPage
        };
    }

    // -------------------------------------------------------------
    // Auto-Detect COM Port & Baud Rate Command
    // -------------------------------------------------------------
    [RelayCommand]
    private async Task AutoDetectAsync()
    {
        if (IsDetecting) return;

        IsDetecting = true;
        AutoDetectButtonText = "SCANNING...";
        TxActive = true;
        BottomStatusMessage = "Scanning serial ports and candidate baud rates for active VAG K-line controllers...";

        try
        {
            var result = await Kw1281TesterWrapper.AutoDetectConnectionAsync(msg =>
            {
                Dispatcher.UIThread.Post(() => BottomStatusMessage = msg);
            });

            if (result.Success)
            {
                if (!AvailablePorts.Contains(result.Port))
                {
                    AvailablePorts.Add(result.Port);
                }
                SelectedPort = result.Port;

                var prefix = result.Baud.ToString();
                foreach (var b in AvailableBauds)
                {
                    if (b.StartsWith(prefix))
                    {
                        SelectedBaud = b;
                        break;
                    }
                }

                BottomStatusMessage = $"LOCKED: Detected {result.PartNumber} on {result.Port} @ {result.Baud} Baud (Address 0x{result.Address:X2}).";
            }
            else
            {
                BottomStatusMessage = "Auto-detect: No active VAG K-line controller detected. Check cable connection & ignition.";
            }
        }
        catch (Exception ex)
        {
            BottomStatusMessage = $"Auto-detect error: {ex.Message}";
        }
        finally
        {
            IsDetecting = false;
            AutoDetectButtonText = "AUTO-DETECT";
            TxActive = false;
        }
    }

    // -------------------------------------------------------------
    // Live Hardware Connection Routine (wrapping kw1281test)
    // -------------------------------------------------------------
    [RelayCommand]
    private async Task ToggleConnectionAsync()
    {
        if (IsConnected)
        {
            IsConnected = false;
            ConnectionStatusText = "CONNECT";
            TxActive = false;
            RxActive = false;
            OverviewPage.Reset();
            BottomStatusMessage = "Disconnected from vehicle.";
            return;
        }

        try
        {
            BottomStatusMessage = $"Opening {SelectedPort}... Initializing 5-baud wake-up on Address 01 via kw1281test...";
            TxActive = true;

            var baud = 10400;
            if (int.TryParse(SelectedBaud.Split(' ')[0], out var parsedBaud))
            {
                baud = parsedBaud;
            }

            var result = await Kw1281TesterWrapper.ConnectControllerAsync(SelectedPort, baud, 0x01);
            if (result.Success)
            {
                RxActive = true;
                IsConnected = true;
                ConnectionStatusText = "DISCONNECT";
                OverviewPage.UpdateFromEcu(result.PartNumber, result.Component, result.Coding, result.WorkshopCode);
                BottomStatusMessage = $"CONNECTED: {result.PartNumber} {result.Component} [kw1281test engine]";
            }
            else
            {
                TxActive = false;
                RxActive = false;
                BottomStatusMessage = $"Wake-up failed: {result.Error}";
            }
        }
        catch (Exception ex)
        {
            TxActive = false;
            RxActive = false;
            BottomStatusMessage = $"Connection error: {ex.Message}";
        }
    }
}
