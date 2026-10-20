# Chunk 139 — `c_bind.cpp` + `DoKeySetup` binding load

`VmKeyBindings`, `VmKeyNames` (+ generated `KeyNames` table), and `VmGameConfigKeySetup` load `Bindings`/`DoubleBindings`/`AutomapBindings` sections.

**Not credited:** `KeyNames[]` maintenance beyond lookup table, `PerformBind` CCMDs, joystick `I_GetJoysticks` reset (chunk 140).

Tests: `VmKeyBindingsTests`, `VmGameConfigKeySetupTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
