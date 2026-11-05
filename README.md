# Fansjoy S1 Pro support for OpenTabletDriver

Adds full support for the Fansjoy S1 Pro (`FJ-85`, USB `2D80:8011`) to OpenTabletDriver 0.6.8/0.6.8.1.

Validated against a connected S1 Pro with OpenTabletDriver 0.6.8.1. See [RE_NOTES.md](RE_NOTES.md) for the recovered protocol and evidence boundary.

Supported inputs:

- Pen position and 8191 pressure levels
- Two pen buttons and eraser state
- X/Y tilt
- All 18 tablet buttons

## Install

1. Install or extract [OpenTabletDriver 0.6.8.1](https://github.com/OpenTabletDriver/OpenTabletDriver/releases/tag/v0.6.8.1).
2. Exit both OpenTabletDriver and the OEM `PenDriver` application. Do not run both drivers together.
3. Run:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\Install.ps1
   ```

   For a portable OpenTabletDriver installation, pass its `userdata` directory:

   ```powershell
   .\Install.ps1 -OpenTabletDriverData 'C:\path\to\OpenTabletDriver\userdata'
   ```

4. Start OpenTabletDriver and select **Detect Tablet**. It should report `Fansjoy S1 Pro`.

The installer writes only these files:

- `Configurations\Fansjoy S1 Pro.json`
- `Plugins\FansjoyS1Pro\OpenTabletDriver.FansjoyS1Pro.dll`

## Build

Requires the .NET 10 SDK:

```powershell
.\Build.ps1
```

The output is written to `dist`. `OpenTabletDriver.FansjoyS1Pro.zip` can also be installed through OpenTabletDriver's plugin manager; the JSON configuration must still be copied to the `Configurations` directory.

## Recovered protocol

The OEM driver uses the vendor HID interface (`usage FF00:FF00`, 64-byte input reports) and sends feature report `20 01` to enable it.

| Report | Layout |
| --- | --- |
| `01` / `0A` | state, X `u16le`, Y `u16le`, pressure `u16le`, tilt X/Y `i16le` in 0.01 degrees |
| `02` | 18 tablet-button bytes beginning at offset 2; the following two entries belong to the pen |

Feature report `12` returned maxima `21000 × 13000` and pressure `8191`. Feature report `15` returned 20 controls: 18 tablet buttons and two pen buttons.
