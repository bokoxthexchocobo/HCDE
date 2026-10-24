# Chunk 140 — bind-reset flags

`VmGameConfigResetBindFlags` queues `V226GamePad` / `V230Gamma` from `LastRun` version; `DoKeySetup` applies gamma bind swap (`bumpgamma` → `F11` `bumplight`).

**Not credited:** full gamepad axis rebind table (`C_SetDefaultBindings` with axis list).

Tests: `VmGameConfigKeySetupTests`.

Verified: `dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`.
