namespace KTachoStudio.Core.Models;

public record OperationProgress(
    int Percent,
    string StatusMessage,
    double SpeedKbPerSec = 0,
    int CurrentSector = 0,
    int TotalSectors = 16
);
