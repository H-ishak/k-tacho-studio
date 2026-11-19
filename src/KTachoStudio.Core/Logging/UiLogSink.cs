using System;
using BitFab.KW1281Test.Logging;

namespace KTachoStudio.Core.Logging;

/// <summary>
/// Log sink adapter that implements kw1281test's ILog interface
/// and routes all diagnostic output directly to UI subscribers.
/// </summary>
internal sealed class UiLogSink : ILog
{
    public static UiLogSink Instance { get; } = new();

    public event Action<string>? LogReceived;

    private UiLogSink()
    {
    }

    public void Write(string message, LogDest dest = LogDest.All)
    {
        LogReceived?.Invoke(message);
    }

    public void WriteLine(LogDest dest = LogDest.All)
    {
        LogReceived?.Invoke(string.Empty);
    }

    public void WriteLine(string message, LogDest dest = LogDest.All)
    {
        LogReceived?.Invoke(message);
    }

    public void Close()
    {
    }

    public void Dispose()
    {
    }
}
