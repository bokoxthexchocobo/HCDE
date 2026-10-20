# Chunk 402 — D_DoomInit, AddAutoloadFiles, CheckCmdLine dmflags

## Scope

D_DoomInit, AddAutoloadFiles, CheckCmdLine dmflags

## Native ranges

- `src/d_main.cpp` **D_DoomInit** 2488–2505 (18 lines)
- `src/d_main.cpp` **AddAutoloadFiles** 2513–2586 (74 lines)
- `src/d_main.cpp` **CheckCmdLine** 2594–2680 (87 lines)

**Credited native LOC:** 179 (tally `nativeLoc` 179).

## Managed

`VmDMainStartupCmdline.cs`, `VmDMainStartupCmdlineFlags.cs`, `VmDMainStartupCmdlineTests.cs`
Managed LOC (tally): 148.
Conversion commit: `dcf7c073ad8176a1a8afc41ae91df103990eefce`.

## Tests

- `VmDMainStartupCmdlineTests.ShouldLoadLightsPk3_WhenStartupRequests`
- `VmDMainStartupCmdlineTests.TryParseExplicitGametype_PlusSetForm`
- `VmDMainStartupCmdlineTests.ShouldResetSoloGametype_WhenLocalSolo`
- `VmDMainStartupCmdlineTests.ApplyCoopMode_SetsDf3LocalItems_WhenNotHexen`
- `VmDMainStartupCmdlineTests.ApplyMonsterFlags_SetsNoMonsters`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 402 --to 402` (0 hard / 0 soft).

## Seams

`Args`, `M_LoadDefaults`, and `Net_TraceSet*` are not hosted.

## Credit rationale

Credits `D_DoomInit`, `AddAutoloadFiles`, and the opening `CheckCmdLine` dmflags/gametype logic from `d_main.cpp`.
