using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KTachoStudio.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // -------------------------------------------------------------
    // Connection & Hardware State
    // -------------------------------------------------------------
    [ObservableProperty]
    private string _selectedPort = "COM3";

    [ObservableProperty]
    private ObservableCollection<string> _availablePorts = new() { "COM1", "COM3", "COM4", "/dev/ttyUSB0", "FTDI D2XX" };

    [ObservableProperty]
    private string _selectedBaud = "10400 (Standard)";

    [ObservableProperty]
    private ObservableCollection<string> _availableBauds = new() { "10400 (Standard)", "9600 (Marelli)", "19200", "38400", "57600 (High Speed)" };

    [ObservableProperty]
    private bool _isConnected = false;

    [ObservableProperty]
    private string _connectionStatusText = "DISCONNECTED";

    [ObservableProperty]
    private string _batteryVoltage = "13.8V Bench";

    [ObservableProperty]
    private bool _isBatteryVoltageOk = true;

    // -------------------------------------------------------------
    // Navigation
    // -------------------------------------------------------------
    [ObservableProperty]
    private int _selectedTabIndex = 0;

    // -------------------------------------------------------------
    // Controller Info (Ident)
    // -------------------------------------------------------------
    [ObservableProperty]
    private string _controllerPartNumber = "038 906 019 HJ";

    [ObservableProperty]
    private string _controllerComponent = "1.9l R4 EDC 0000SG  1536";

    [ObservableProperty]
    private string _controllerCoding = "00002";

    [ObservableProperty]
    private string _controllerWsc = "01234";

    [ObservableProperty]
    private string _controllerVin = "WVWZZZ1JZ1W012345";

    [ObservableProperty]
    private string _controllerProtocol = "KWP1281 / KWP2000";

    [ObservableProperty]
    private string _securityAccessStatus = "Level 41 Authenticated";

    // -------------------------------------------------------------
    // EDC15 / EDC16 Tuning Workbench
    // -------------------------------------------------------------
    [ObservableProperty]
    private string _ecuModel = "Bosch EDC15P+ (C167 + 29F400BT)";

    [ObservableProperty]
    private string _flashFilePath = "D:\\Tuning\\Golf4_1.9TDI_130_Stage1.bin";

    [ObservableProperty]
    private string _flashFileSize = "512 KB (16 Sectors)";

    [ObservableProperty]
    private string _selectedFlashSpeed = "Medium (Fast Link)";

    [ObservableProperty]
    private bool _fastInitEnabled = false;

    [ObservableProperty]
    private bool _smartSectorCheck = true;

    [ObservableProperty]
    private bool _verifyChecksum = true;

    [ObservableProperty]
    private int _flashProgressPercent = 42;

    [ObservableProperty]
    private string _flashOperationStatus = "Reading Sector 0x06 (42%) - 38.4 KB/s";

    // -------------------------------------------------------------
    // Cluster & Immo
    // -------------------------------------------------------------
    [ObservableProperty]
    private string _clusterType = "VDO V08 (Golf Mk4 / Bora)";

    [ObservableProperty]
    private string _clusterPin = "04821";

    [ObservableProperty]
    private string _clusterMileage = "184,320 km";

    [ObservableProperty]
    private string _immoGeneration = "Immobilizer 3 (6-pin seed/key)";

    [ObservableProperty]
    private string _targetClusterPin = "01234";

    // -------------------------------------------------------------
    // Live Diagnostics & Telemetry
    // -------------------------------------------------------------
    [ObservableProperty]
    private int _measuringGroup = 3;

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

    // -------------------------------------------------------------
    // Comfort (CCM) Features
    // -------------------------------------------------------------
    [ObservableProperty]
    private bool _windowRollUpEnabled = true;

    [ObservableProperty]
    private bool _alarmChirpEnabled = true;

    [ObservableProperty]
    private bool _autoLockEnabled = true;

    [ObservableProperty]
    private bool _selectiveUnlock = false;

    // -------------------------------------------------------------
    // Bottom Telemetry & Console
    // -------------------------------------------------------------
    [ObservableProperty]
    private bool _txActive = true;

    [ObservableProperty]
    private bool _rxActive = false;

    [ObservableProperty]
    private string _bottomStatusMessage = "Ready - Port COM3 open at 10400 baud. Connected to 01 (Engine).";

    [ObservableProperty]
    private string _radioSafeCode = "1984";

    // -------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------
    [RelayCommand]
    private void ToggleConnection()
    {
        IsConnected = !IsConnected;
        ConnectionStatusText = IsConnected ? "CONNECTED" : "DISCONNECTED";
        BottomStatusMessage = IsConnected 
            ? $"Connected to {SelectedPort} ({SelectedBaud}) - ECU Ident: {ControllerPartNumber}" 
            : "Disconnected from vehicle.";
    }

    [RelayCommand]
    private void SwitchTab(string index)
    {
        if (int.TryParse(index, out var idx))
        {
            SelectedTabIndex = idx;
        }
    }

    [RelayCommand]
    private void DumpFlash()
    {
        BottomStatusMessage = "Initiating high-speed Flash dump over K-Line...";
    }

    [RelayCommand]
    private void WriteFlash()
    {
        BottomStatusMessage = "Safety check passed. Flashing modified sectors to EDC15...";
    }

    [RelayCommand]
    private void ReadPin()
    {
        BottomStatusMessage = "Querying VDO Cluster Security Access... PIN extracted: " + ClusterPin;
    }
}
