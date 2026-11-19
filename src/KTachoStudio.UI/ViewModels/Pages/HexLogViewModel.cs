using System;
using System.Text;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KTachoStudio.Core.Adapters;

namespace KTachoStudio.UI.ViewModels.Pages;

public partial class HexLogViewModel : ViewModelBase
{
    private readonly StringBuilder _logBuilder = new();

    [ObservableProperty]
    private string _hexDumpText = "No binary data loaded. Use Load BIN or dump memory to inspect hex.";

    [ObservableProperty]
    private string _klineLogText = "[INIT] kw1281test engine ready. Select port and connect to capture live K-line packets.\n";

    public HexLogViewModel()
    {
        Kw1281TesterWrapper.OnLogMessage += OnLogReceived;
    }

    private void OnLogReceived(string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _logBuilder.AppendLine($"[{timestamp}] {message}");

            // Keep buffer bounded
            if (_logBuilder.Length > 200_000)
            {
                _logBuilder.Remove(0, 50_000);
            }

            KlineLogText = _logBuilder.ToString();
        });
    }

    public void UpdateHexDump(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            HexDumpText = "Empty buffer.";
            return;
        }

        var sb = new StringBuilder();
        var maxBytes = Math.Min(data.Length, 4096); // Preview first 4KB
        for (var i = 0; i < maxBytes; i += 16)
        {
            sb.Append($"{i:X8}: ");
            for (var j = 0; j < 16; j++)
            {
                if (i + j < maxBytes)
                    sb.Append($"{data[i + j]:X2} ");
                else
                    sb.Append("   ");
            }
            sb.Append(" ");
            for (var j = 0; j < 16 && (i + j) < maxBytes; j++)
            {
                var b = data[i + j];
                sb.Append(b >= 32 && b < 127 ? (char)b : '.');
            }
            sb.AppendLine();
        }

        HexDumpText = sb.ToString();
    }

    [RelayCommand]
    private void ClearLog()
    {
        _logBuilder.Clear();
        KlineLogText = string.Empty;
    }

    [RelayCommand]
    private void LoadBin()
    {
    }

    [RelayCommand]
    private void SaveBin()
    {
    }
}
