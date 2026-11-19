using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BitFab.KW1281Test;
using BitFab.KW1281Test.EDC15;
using KTachoStudio.Core.Hardware;
using KTachoStudio.Core.Logging;

namespace KTachoStudio.Core.Adapters;

public record AutoDetectResult(
    bool Success,
    string Port,
    int Baud,
    int Address,
    string PartNumber,
    string Component,
    string? Error = null
);

/// <summary>
/// Direct wrapper around upstream kw1281test engine.
/// Delegates all ECU/Cluster operations to BitFab.KW1281Test.Tester without altering upstream logic.
/// </summary>
public static class Kw1281TesterWrapper
{
    public static event Action<string>? OnLogMessage
    {
        add => UiLogSink.Instance.LogReceived += value;
        remove => UiLogSink.Instance.LogReceived -= value;
    }

    static Kw1281TesterWrapper()
    {
        // Route all upstream kw1281test logging to the UI log sink
        Program.Log = UiLogSink.Instance;
    }

    /// <summary>
    /// Automatically scans serial ports and candidate VAG baud rates to detect an active K-line controller.
    /// </summary>
    public static async Task<AutoDetectResult> AutoDetectConnectionAsync(
        Action<string>? onProgress = null,
        System.Threading.CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var ports = System.IO.Ports.SerialPort.GetPortNames()
                .Distinct()
                .OrderByDescending(p => p != "COM1")
                .ToList();

            if (!ports.Contains("MOCK_SIMULATOR"))
            {
                ports.Add("MOCK_SIMULATOR");
            }

            int[] candidateBauds = [10400, 9600, 4800];
            int[] candidateAddresses = [0x01, 0x17]; // Engine ECU (0x01), Cluster (0x17)

            foreach (var port in ports)
            {
                if (cancellationToken.IsCancellationRequested) break;

                foreach (var baud in candidateBauds)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    foreach (var addr in candidateAddresses)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        var moduleName = addr == 0x01 ? "Engine ECU" : "Cluster";
                        onProgress?.Invoke($"Scanning {port} @ {baud} Baud ({moduleName} $0{addr:X})...");

                        try
                        {
                            using var iface = InterfaceFactory.Open(port, baud, (byte)addr);
                            iface.ReadTimeout = 1200;
                            iface.WriteTimeout = 1200;

                            var tester = new Tester(iface, addr);
                            var ecuInfo = tester.Kwp1281Wakeup(evenParityWakeup: false, failQuietly: true);
                            tester.EndCommunication();

                            if (ecuInfo != null && !string.IsNullOrWhiteSpace(ecuInfo.Text))
                            {
                                var (partNo, comp) = ParseIdentification(ecuInfo.Text);
                                onProgress?.Invoke($"LOCKED: {port} @ {baud} Baud ({partNo} {comp})");
                                return new AutoDetectResult(true, port, baud, addr, partNo, comp, null);
                            }
                        }
                        catch
                        {
                            // Port or baud did not respond, proceed to next candidate
                        }
                    }
                }
            }

            return new AutoDetectResult(false, string.Empty, 0, 0, string.Empty, string.Empty, "No active K-line controller detected.");
        }, cancellationToken);
    }

    /// <summary>
    /// Connects to a controller, retrieves identification metadata, and closes communication.
    /// </summary>
    public static async Task<(bool Success, string PartNumber, string Component, string Coding, string WorkshopCode, string FullText, string? Error)>
        ConnectControllerAsync(string portName, int baudRate, int address = 0x01)
    {
        return await Task.Run<(bool Success, string PartNumber, string Component, string Coding, string WorkshopCode, string FullText, string? Error)>(() =>
        {
            try
            {
                using var iface = InterfaceFactory.Open(portName, baudRate, (byte)address);
                var tester = new Tester(iface, address);

                var ecuInfo = tester.Kwp1281Wakeup(evenParityWakeup: false, failQuietly: false);
                tester.EndCommunication();

                var fullText = ecuInfo.Text ?? string.Empty;
                var (partNo, component) = ParseIdentification(fullText);

                return (
                    true,
                    partNo,
                    component,
                    $"{ecuInfo.SoftwareCoding:D5}",
                    $"{ecuInfo.WorkshopCode:D5}",
                    fullText,
                    null
                );
            }
            catch (Exception ex)
            {
                return (false, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ex.Message);
            }
        });
    }

    /// <summary>
    /// Reads stored Diagnostic Trouble Codes (DTCs) from the target controller.
    /// </summary>
    public static async Task<List<string>> ReadFaultCodesAsync(string portName, int baudRate, int address = 0x01)
    {
        return await Task.Run(() =>
        {
            var results = new List<string>();
            try
            {
                using var iface = InterfaceFactory.Open(portName, baudRate, (byte)address);
                var tester = new Tester(iface, address);

                var session = tester.WakeUpAny();
                var codes = tester.ReadFaultCodesAny(session);
                tester.EndCommunication();

                if (codes != null && codes.Count > 0)
                {
                    foreach (var code in codes)
                    {
                        results.Add(code.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                results.Add($"Fault code read error: {ex.Message}");
            }

            return results;
        });
    }

    /// <summary>
    /// Clears Diagnostic Trouble Codes (DTCs) on the target controller.
    /// </summary>
    public static async Task<bool> ClearFaultCodesAsync(string portName, int baudRate, int address = 0x01)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var iface = InterfaceFactory.Open(portName, baudRate, (byte)address);
                var tester = new Tester(iface, address);

                var session = tester.WakeUpAny();
                tester.ClearFaultCodesAny(session);
                tester.EndCommunication();
                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Retrieves instrument cluster PIN/SKC by delegating directly to upstream Tester.GetSkc().
    /// </summary>
    public static async Task<(bool Success, string Skc, string LogOutput)> ReadClusterSkcAsync(string portName, int baudRate)
    {
        return await Task.Run(() =>
        {
            var capturedSkc = string.Empty;
            Action<string> logCapture = msg =>
            {
                var match = Regex.Match(msg, @"SKC:\s*(\d+)");
                if (match.Success)
                {
                    capturedSkc = match.Groups[1].Value.PadLeft(5, '0');
                }
            };

            UiLogSink.Instance.LogReceived += logCapture;
            try
            {
                using var iface = InterfaceFactory.Open(portName, baudRate, 0x17);
                var tester = new Tester(iface, 0x17);
                tester.GetSkc();
                tester.EndCommunication();

                return (!string.IsNullOrEmpty(capturedSkc), capturedSkc, "SKC query completed.");
            }
            catch (Exception ex)
            {
                return (false, string.Empty, ex.Message);
            }
            finally
            {
                UiLogSink.Instance.LogReceived -= logCapture;
            }
        });
    }

    /// <summary>
    /// Reads EDC15 Flash to a file using upstream Tester.DumpEdc15Flash.
    /// </summary>
    public static async Task<bool> DumpEdc15FlashAsync(string portName, int baudRate, string filename, Action<int>? onProgress = null)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var iface = InterfaceFactory.Open(portName, baudRate, 0x01);
                var tester = new Tester(iface, 0x01);

                tester.DumpEdc15Flash(
                    Edc15FlashVM.Variant.V,
                    filename,
                    flashSpeed: Edc15FlashVM.FlashSpeed.Medium);

                return File.Exists(filename);
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Writes EDC15 Flash from a file using upstream Tester.LoadEdc15Flash.
    /// </summary>
    public static async Task<bool> LoadEdc15FlashAsync(string portName, int baudRate, string filename, Action<int>? onProgress = null)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var iface = InterfaceFactory.Open(portName, baudRate, 0x01);
                var tester = new Tester(iface, 0x01);

                tester.LoadEdc15Flash(
                    Edc15FlashVM.Variant.V,
                    filename,
                    flashSpeed: Edc15FlashVM.FlashSpeed.Medium,
                    verify: true);

                return true;
            }
            catch
            {
                return false;
            }
        });
    }

    /// <summary>
    /// Dumps EDC15 EEPROM (512 or 1024 bytes) using upstream Tester.ReadWriteEdc15Eeprom.
    /// </summary>
    public static async Task<byte[]?> DumpEdc15EepromAsync(string portName, int baudRate, string filename)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var iface = InterfaceFactory.Open(portName, baudRate, 0x01);
                var tester = new Tester(iface, 0x01);

                var bytes = tester.ReadWriteEdc15Eeprom(filename);
                tester.EndCommunication();
                return bytes;
            }
            catch
            {
                return null;
            }
        });
    }

    private static (string PartNumber, string Component) ParseIdentification(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return ("-", "-");

        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var firstLine = lines.Length > 0 ? lines[0].Trim() : text.Trim();

        // Regex for VW part numbers: e.g. 038906019HJ, 1J0920826C
        var match = Regex.Match(firstLine, @"^([0-9A-Z]{9,11})\s*(.*)$");
        if (match.Success)
        {
            return (match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim());
        }

        return (firstLine, "-");
    }
}
