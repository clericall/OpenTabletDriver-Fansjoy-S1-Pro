using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.FansjoyS1Pro;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public sealed class FansjoyS1ProReportParser : IReportParser<IDeviceReport>
{
    public IDeviceReport Parse(byte[] report)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (report.Length == 0)
            return new DeviceReport(report);

        return report[0] switch
        {
            0x01 or 0x0a when report.Length >= FansjoyS1ProTabletReport.MinimumReportLength =>
                report[1] != 0 ? new FansjoyS1ProTabletReport(report) : new OutOfRangeReport(report),
            0x02 when report.Length >= FansjoyS1ProAuxReport.MinimumReportLength =>
                new FansjoyS1ProAuxReport(report),
            _ => new DeviceReport(report),
        };
    }
}
