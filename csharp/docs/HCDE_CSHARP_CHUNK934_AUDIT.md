# Chunk 934 — `LS_SetGlobalFogParameter` close, glow/health specials, and `LineSpecials[]` through opcode **57** (`p_lnspec.cpp` **3455–3604**). Follows **933** (**≤3454**); dispatch continuation at **3605+** deferred.

## Scope

`LS_SetGlobalFogParameter` close, glow/health specials, and `LineSpecials[]` through opcode **57** (`p_lnspec.cpp` **3455–3604**). Follows **933** (**≤3454**); dispatch continuation at **3605+** deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_SetGlobalFogParameter** 3455–3604 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecEnvGlowHealth934Slice.cs`
Managed LOC (tally): 147.
Conversion commit: `a2db5067693cf20e3ca834d19212dddbc28976ae`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `GlobalFogSwitchExhausted` | `LS_SetGlobalFogParameter` default **3453–3456** |
| `BuildFloorGlow` / `ResolveGlowColor` | `LS_Sector_SetFloorGlow` **3458–3473** |
| `BuildCeilingGlow` | `LS_Sector_SetCeilingGlow` **3475–3490** |
| `BuildLineHealth` / `ClampLineHealth` | `LS_Line_SetHealth` **3492–3509** |
| `BuildSectorHealth` / `SectorHealthPart` | `LS_Sector_SetHealth` **3511–3543** |
| `LineSpecialDispatchThrough57` | `LineSpecials[]` **3545–3604** |
| `DispatchName` / `DispatchIsNop` | table lookup **3547–3604** |
| `DispatchIsDoorCluster` | door opcodes **3557–3561** |
| `PackGlowRgb` | `PalEntry` **3462**, **3479** |
| negative height invalidates glow | **3463**, **3480** |
| health group propagation | **3505–3506**, **3526–3539** |

### Chunk **934** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**933** |


**Post-review ratio:** 147/150 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecEnvGlowHealth934SliceTests.GlobalFogSwitchExhausted_RejectsUnknownType`
- `VmPLnspecEnvGlowHealth934SliceTests.BuildFloorGlow_InvalidatesColorWhenHeightNegative`
- `VmPLnspecEnvGlowHealth934SliceTests.BuildLineHealth_ClampsNegative`
- `VmPLnspecEnvGlowHealth934SliceTests.BuildSectorHealth_SelectsCeilingPart`
- `VmPLnspecEnvGlowHealth934SliceTests.DispatchName_MapsGlassBreakOpcode`
- `VmPLnspecEnvGlowHealth934SliceTests.DispatchIsDoorCluster_RecognizesRaiseRange`
- `VmPLnspecEnvGlowHealth934SliceTests.CountDispatchEntries_MatchesNativeHead`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 934 --to 934` (0 hard / 0 soft).

## Seams

Sector/line iterators, `PalEntry`, `P_SetHealthGroupHealth`, and `lnSpecFunc` host binding remain seams.

## Credit rationale

Honest **150**-line UNCredited band toward **3982**; exclusive `VmPLnspecEnvGlowHealth934Slice.cs`.
