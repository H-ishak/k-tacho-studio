using System.Collections.Generic;
using System.Threading.Tasks;
using KTachoStudio.Core.Adapters;
using Xunit;

namespace KTachoStudio.Core.Tests;

public class Kw1281WrapperTests
{
    [Fact]
    public async Task ConnectControllerAsync_WithMockEdc15_ReturnsExpectedMetadata()
    {
        var logs = new List<string>();
        Kw1281TesterWrapper.OnLogMessage += l => logs.Add(l);

        var result = await Kw1281TesterWrapper.ConnectControllerAsync("MOCK_SIMULATOR", 10400, 0x01);

        var combinedLogs = string.Join("\n", logs);
        Assert.True(result.Success, $"Expected connection success but got error: {result.Error}\nLogs:\n{combinedLogs}");
        Assert.Equal("038906019HJ", result.PartNumber);
        Assert.Contains("1.9l R4 EDC", result.Component);
        Assert.Equal("00002", result.Coding);
        Assert.Equal("01234", result.WorkshopCode);
    }

    [Fact]
    public async Task OnLogMessage_CapturesUpstreamDiagnosticMessages()
    {
        var capturedLogs = new List<string>();
        void OnLog(string line) => capturedLogs.Add(line);

        Kw1281TesterWrapper.OnLogMessage += OnLog;
        try
        {
            var result = await Kw1281TesterWrapper.ConnectControllerAsync("MOCK_SIMULATOR", 10400, 0x01);
            Assert.True(result.Success, $"Failed with: {result.Error}");
            Assert.NotEmpty(capturedLogs);
            Assert.Contains(capturedLogs, l => l.Contains("wakeup") || l.Contains("Reading") || l.Contains("ECU:"));
        }
        finally
        {
            Kw1281TesterWrapper.OnLogMessage -= OnLog;
        }
    }

    [Fact]
    public async Task AutoDetectConnectionAsync_FindsActiveController()
    {
        var scanSteps = new List<string>();
        var result = await Kw1281TesterWrapper.AutoDetectConnectionAsync(msg => scanSteps.Add(msg));

        Assert.True(result.Success, $"Expected auto-detection success, error: {result.Error}");
        Assert.NotEmpty(result.Port);
        Assert.Equal(10400, result.Baud);
        Assert.Equal("038906019HJ", result.PartNumber);
        Assert.NotEmpty(scanSteps);
    }
}
