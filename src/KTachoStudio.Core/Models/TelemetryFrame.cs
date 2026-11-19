using System;

namespace KTachoStudio.Core.Models;

public record TelemetryFrame(
    DateTime Timestamp,
    int Rpm,
    double BoostMbar,
    double MafMgPerStr,
    double InjectionQtyMgPerStr,
    double CoolantTempC
);
