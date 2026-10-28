# ACS print / HUD opcodes — chunk 14 audit

## Scope

`AcsPrintHudOpcodes` and `AcsVm` wire-up for `PCD_BEGINPRINT` through `PCD_ENDHUDMESSAGEBOLD`, char-array print variants, `SetFont`/`SetStyle`/`SetHudSize`, and `EndLog`. HUD messages are recorded on `AuthoritySimulation` for regression tests (presentation layer consumes separately).

**Conversion native LOC credited:** 1,548.

## Tests

- `AcsPrintHudOpcodeTests.BeginPrint_EndPrint_FlushesToBuffer`
- `AcsPrintHudOpcodeTests.PrintString_UsesStringTable`
- `AcsPrintHudOpcodeTests.EndHudMessage_RecordsHudPayload`
- `AcsPrintHudOpcodeTests.PrintBinaryAndHex_UsePrintBuilder`
- `AcsAbsentOpcodeChunk10Tests.PrintBinaryAndHex_AppendToBuffer` (updated for BeginPrint/EndPrint)

## Verified

`dotnet build -warnaserror`; full `dotnet test` (22,187 passed).
