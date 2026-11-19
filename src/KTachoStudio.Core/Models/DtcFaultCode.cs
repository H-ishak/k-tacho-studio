namespace KTachoStudio.Core.Models;

public record DtcFaultCode(
    int Code,
    string VwsCode,
    string Description,
    string Status
)
{
    public override string ToString() => $"{Code} ({VwsCode}) - {Description} [{Status}]";
}
