# Reverse-engineering notes

## Observed

- OEM Electron package: `resources/app.asar`, SHA-256 `a46c2b57f6e1068d6a146a0c9d5ebfa53440914cdf39bb0687cacb0442a8e5a1`.
- REA static-analysis parent: `ev_34b05203d54a152cc2cd42fae1871168e3246d6a86f12c4d4f453c1596afe5d1`; extraction: `ev_a3b03341ca1168a59acf5e05c73063fa7b91a459215e41ed38e12ca00247b940`.
- The OEM `main.js` opens `VID 2D80`, ignores `PID 0001`, and selects the `FF00:FF00` HID interface. It reads features `11`, `12`, and `15`, enables reports with feature `20 01`, and parses reports `01`/`0A` and `02`.
- Connected hardware identifies as manufacturer `Fansjoy`, product `FJ-85`, `VID:PID 2D80:8011`. The selected vendor interface has 64-byte input reports and 33-byte feature reports.
- Feature `12`: `08 52 C8 32 FF 1F ...`, giving X max 21000, Y max 13000, pressure max 8191.
- Feature `15`: first byte `14` (20 controls), followed by 18 tablet-key defaults and two pen-button defaults.
- The physical working area follows the same 100 units/mm convention as existing OpenTabletDriver Fansjoy/XENX configurations: 210 × 130 mm.
- `pendriver.sys`, SHA-256 `ab830fb02248059f7cfa3dc1e340067dd18a0d39626bc747c4b09ae03974bf5a`, contains a digitizer descriptor at file offset `0x3e38`. It defines pressure 0..8191 and signed X/Y tilt -9000..9000 with a 10^-2 degree exponent.

## Implementation choices

- Reports `01` and `0A` use the same pen decoder because the OEM application handles both with one layout.
- Report `02` exposes bytes 2..19 as the 18 auxiliary buttons. Bytes 20 and 21 are excluded because the OEM feature table identifies them as the two pen controls.
- The configuration uses the standard Fansjoy/XENX feature initialization (`20 01`) but a dedicated parser so all 18 keys and tilt are available.

## Validation

- Plugin builds cleanly against `OpenTabletDriver.Plugin` 0.6.8 on .NET 10.
- Synthetic parser checks cover position, pressure, signed tilt, pen buttons, out-of-range reports, and first/last auxiliary buttons.
- OpenTabletDriver 0.6.8.1 loaded the plugin on Windows, selected the 64-byte vendor interface, sent `20 01`, and reported `Found tablet 'Fansjoy S1 Pro'` with a 210 × 130 mm area and 18 auxiliary bindings.

## Remaining boundary

- One connected S1 Pro firmware revision was tested. Other devices sold under the same name may need an additional product ID or report-layout variant.
