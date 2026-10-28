# Parked — stringtable / c_bind / c_cvars tail work

The following native areas are intentionally **not** scheduled for further small-chunk conversion (diminishing returns). Future work should be full-subsystem ports or live integration seams only.

## stringtable.cpp

- `FStringTable::GetString` developer miss logging (`language_debug_maxlen`, missed map)
- `sc.isText()` binary lump path vs NUL heuristic
- Full `FileSystem` `LoadStrings` IWAD/PWAD adapter (managed seam exists: `IVmStringTableLumpSource`)
- Gendered string table columns beyond macro slot 0 in CSV lumps

## c_bind.cpp

- `C_DoKey` command execution (`AddCommandString`), chat-mode filter, `DelayedReleases` synthetic `+` releases
- Global static `Bindings` / `ReadBindings` WAD lump driver (lump text loader is ported)
- `rebind` CCMD and joystick-specific paths

## c_cvars.cpp

- `FColorCVar`, ZScript custom CVars, serverinfo latch gate in `SetGenericRep`
- `C_ReadCVars` / `C_WriteCVars`, demo net vars, `UnlatchCVars` userinfo/serverinfo side effects
- Wiring `VmTypedCvar` latch through `VmCvarRegistry.Create` for live playsim

Verified: no code changes in this audit file.
