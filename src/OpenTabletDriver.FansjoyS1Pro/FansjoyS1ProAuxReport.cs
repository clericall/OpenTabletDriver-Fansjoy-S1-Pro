using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.FansjoyS1Pro;

public struct FansjoyS1ProAuxReport : IAuxReport
{
    public const int ButtonCount = 18;
    public const int MinimumReportLength = ButtonCount + 2;

    public FansjoyS1ProAuxReport(byte[] report)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (report.Length < MinimumReportLength)
            throw new ArgumentException($"Expected at least {MinimumReportLength} bytes.", nameof(report));

        Raw = report;
        AuxButtons = new bool[ButtonCount];
        for (var index = 0; index < AuxButtons.Length; index++)
            AuxButtons[index] = report[index + 2] != 0;
    }

    public byte[] Raw { get; set; }
    public bool[] AuxButtons { get; set; }
}
