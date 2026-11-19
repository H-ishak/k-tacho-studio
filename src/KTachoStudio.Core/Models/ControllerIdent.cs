namespace KTachoStudio.Core.Models;

public record ControllerIdent(
    string PartNumber,
    string Component,
    string Coding,
    string WorkshopCode,
    string? Extra = null
)
{
    public override string ToString() => $"{PartNumber} {Component} (Coding: {Coding}, WSC: {WorkshopCode})";
}
