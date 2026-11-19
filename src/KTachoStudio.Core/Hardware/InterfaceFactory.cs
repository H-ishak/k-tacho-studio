using System;
using System.Text.RegularExpressions;
using System.Threading;
using BitFab.KW1281Test.Interface;

namespace KTachoStudio.Core.Hardware;

public static class InterfaceFactory
{
    private static readonly Regex FtdiSerialRegex = new(@"^[A-Z0-9]{8}$", RegexOptions.Compiled);

    /// <summary>
    /// Opens the specified interface (Mock, FTDI D2XX, or standard SerialPort COM port).
    /// </summary>
    public static IInterface Open(string portName, int baudRate, byte targetAddress = 0x01)
    {
        if (string.Equals(portName, "MOCK_SIMULATOR", StringComparison.OrdinalIgnoreCase))
        {
            var sim = new SimulatedInterface();
            sim.SetTargetAddress(targetAddress);
            return sim;
        }

        IInterface iface;
        if (FtdiSerialRegex.IsMatch(portName.ToUpperInvariant()))
        {
            iface = new FtdiInterface(portName, baudRate);
        }
        else
        {
            iface = new GenericInterface(portName, baudRate);
        }

        // Many VAG KKL cables power the K-line transceiver from DTR.
        // Pulse it low then high to ensure a clean power-on state.
        iface.SetDtr(false);
        Thread.Sleep(300);
        iface.SetDtr(true);
        Thread.Sleep(300);

        return iface;
    }
}
