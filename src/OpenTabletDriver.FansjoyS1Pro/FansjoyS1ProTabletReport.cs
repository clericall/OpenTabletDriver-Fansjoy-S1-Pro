using System.Buffers.Binary;
using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.FansjoyS1Pro;

public struct FansjoyS1ProTabletReport : ITabletReport, ITiltReport, IEraserReport
{
    public const int MinimumReportLength = 12;

    public FansjoyS1ProTabletReport(byte[] report)
    {
        ArgumentNullException.ThrowIfNull(report);
        if (report.Length < MinimumReportLength)
            throw new ArgumentException($"Expected at least {MinimumReportLength} bytes.", nameof(report));

        Raw = report;
        Position = new Vector2(
            BinaryPrimitives.ReadUInt16LittleEndian(report.AsSpan(2, 2)),
            BinaryPrimitives.ReadUInt16LittleEndian(report.AsSpan(4, 2))
        );
        Pressure = BinaryPrimitives.ReadUInt16LittleEndian(report.AsSpan(6, 2));
        Tilt = new Vector2(
            BinaryPrimitives.ReadInt16LittleEndian(report.AsSpan(8, 2)) / 100f,
            BinaryPrimitives.ReadInt16LittleEndian(report.AsSpan(10, 2)) / 100f
        );

        var state = report[1];
        PenButtons =
        [
            state.IsBitSet(1),
            state.IsBitSet(2),
        ];
        Eraser = state.IsBitSet(6);
    }

    public byte[] Raw { get; set; }
    public Vector2 Position { get; set; }
    public uint Pressure { get; set; }
    public Vector2 Tilt { get; set; }
    public bool[] PenButtons { get; set; }
    public bool Eraser { get; set; }
}
