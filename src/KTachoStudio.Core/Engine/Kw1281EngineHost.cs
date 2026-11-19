global using static BitFab.KW1281Test.Program;
using System.Collections.Generic;

namespace BitFab.KW1281Test;

/// <summary>
/// Provides the static Log host required by kw1281test engine components,
/// allowing KTachoStudio to direct all diagnostic logs to the UI log sink.
/// </summary>
internal static class Program
{
    public static Logging.ILog Log { get; set; } = new Logging.ConsoleLog();

    internal static List<string> CommandAndArgs { get; set; } = [];
}
