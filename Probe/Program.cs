using HidSharp;
using OpenTabletDriver.FansjoyS1Pro;
using OpenTabletDriver.Plugin.Tablet;

const int vendorId = 0x2d80;
const int productId = 0x8011;

RunParserChecks();

var devices = DeviceList.Local.GetHidDevices(vendorId, productId).ToArray();
Console.WriteLine($"Found {devices.Length} HID interface(s) for {vendorId:x4}:{productId:x4}.");

foreach (var device in devices)
{
    Console.WriteLine();
    Console.WriteLine(device.DevicePath);
    Console.WriteLine($"  Product: {TryGet(device.GetProductName)}");
    Console.WriteLine($"  Manufacturer: {TryGet(device.GetManufacturer)}");
    Console.WriteLine($"  Serial: {TryGet(device.GetSerialNumber)}");
    Console.WriteLine($"  Reports: input={device.GetMaxInputReportLength()}, output={device.GetMaxOutputReportLength()}, feature={device.GetMaxFeatureReportLength()}");
    Console.WriteLine($"  Report descriptor: {Convert.ToHexString(device.GetRawReportDescriptor())}");

    if (device.GetMaxFeatureReportLength() < 12 || !device.TryOpen(out var stream))
        continue;

    using (stream)
    {
        foreach (var reportId in new byte[] { 0x11, 0x12, 0x15 })
        {
            var report = new byte[device.GetMaxFeatureReportLength()];
            report[0] = reportId;
            try
            {
                stream.GetFeature(report);
                Console.WriteLine($"  Feature {reportId:x2}: {Convert.ToHexString(report)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Feature {reportId:x2}: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}

static string TryGet(Func<string> getter)
{
    try
    {
        return getter();
    }
    catch (Exception ex)
    {
        return $"<{ex.GetType().Name}>";
    }
}

static void RunParserChecks()
{
    var parser = new FansjoyS1ProReportParser();
    var penData = new byte[64];
    penData[0] = 0x01;
    penData[1] = 0x27;
    WriteUInt16(penData, 2, 21000);
    WriteUInt16(penData, 4, 13000);
    WriteUInt16(penData, 6, 8191);
    WriteInt16(penData, 8, -4500);
    WriteInt16(penData, 10, 6000);

    var pen = Expect<FansjoyS1ProTabletReport>(parser.Parse(penData));
    Assert(pen.Position.X == 21000 && pen.Position.Y == 13000, "position decoding");
    Assert(pen.Pressure == 8191, "pressure decoding");
    Assert(pen.Tilt.X == -45 && pen.Tilt.Y == 60, "tilt decoding");
    Assert(pen.PenButtons is [true, true], "pen button decoding");

    penData[1] = 0;
    Expect<OutOfRangeReport>(parser.Parse(penData));

    var auxData = new byte[64];
    auxData[0] = 0x02;
    auxData[2] = 1;
    auxData[19] = 1;
    auxData[20] = 1; // Pen button from the vendor's 20-button feature table; not an aux key.
    var aux = Expect<FansjoyS1ProAuxReport>(parser.Parse(auxData));
    Assert(aux.AuxButtons.Length == 18, "aux button count");
    Assert(aux.AuxButtons[0] && aux.AuxButtons[17], "aux button decoding");

    Console.WriteLine("Parser self-test passed.");
}

static T Expect<T>(IDeviceReport report) where T : IDeviceReport
{
    if (report is T expected)
        return expected;
    throw new InvalidOperationException($"Expected {typeof(T).Name}, got {report.GetType().Name}.");
}

static void Assert(bool condition, string name)
{
    if (!condition)
        throw new InvalidOperationException($"Failed parser check: {name}.");
}

static void WriteUInt16(byte[] data, int offset, ushort value)
{
    data[offset] = (byte)value;
    data[offset + 1] = (byte)(value >> 8);
}

static void WriteInt16(byte[] data, int offset, short value) => WriteUInt16(data, offset, unchecked((ushort)value));
