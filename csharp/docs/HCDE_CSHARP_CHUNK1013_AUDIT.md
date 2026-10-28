# Chunk 1013 — `GetSecretExitMap`, `ExitLevel`, `SecretExitLevel`, and early `CreateIntermission` from `g_level.cpp` (**901–1050**); follows deferred **451–582** / chunk **8** gap and prior **1010–1012** bands.

## Scope

`GetSecretExitMap`, `ExitLevel`, `SecretExitLevel`, and early `CreateIntermission` from `g_level.cpp` (**901–1050**); follows deferred **451–582** / chunk **8** gap and prior **1010–1012** bands.

## Native ranges

- `src/g_level.cpp` **GetSecretExitMap** 901–1050 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGLevelExitIntermission1013Slice.cs`
Managed LOC (tally): 193.
Conversion commit: `5ce461a09e1dd2cd6f4ce03817f7d66683d43556`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ChangeLevel` VM arity (**896–905**, seam **901–905**) | yes |
| `GetSecretExitMap` (**912–924**) | yes |
| `ExitLevel` / `SecretExitLevel` (**932–969**) | yes |
| `CreateIntermission` cheat/endgame/Strife (**977–1028**) | yes |
| Non-DM exit text branch start (**1030–1050**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **880–895** | prior `ChangeLevel` coop tail (**452+** band) |
| **975** | `staticWmInfo` host global |
| **1009–1017**, **1042–1050** | `F_StartFinale` host calls (args only where modeled) |
| **1051+** | cluster enter text / remainder of `CreateIntermission` |

| Managed (`VmGLevelExitIntermission1013Slice.cs`) | Native (`g_level.cpp`) |
|--------------------------------------------------|----------------------|
| Secret map resolution | **912–924** |
| `flags3` exit used + change flags | **932–956** |
| Endgame / Strife sequence pick | **988–1004** |
| Exit text key selection | **1006–1036** |
| Band hull (exclusive slice) | **901–1050** |

### Chunk **1013** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1012** |


**Post-review ratio:** 193/150 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmGLevelExitIntermission1013SliceTests.SecretExitMapAndNormalExit`
- `VmGLevelExitIntermission1013SliceTests.EndgameSequenceAndStrifeEnding`
- `VmGLevelExitIntermission1013SliceTests.IntermissionExitTextKeys`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1013 --to 1013` (0 hard / 0 soft).

## Seams

`ChangeLevel` body, `P_CheckMapData`, `FindClusterInfo`, `F_StartFinale`, inventory lookup, and `FindLevelInfo` remain injected; only level-local exit flags and intermission gates are modeled.

## Credit rationale

UNCredited **901–1050**; no `p_saveg` overlap; disjoint from `p_lnspec` exit specials (host `ChangeLevel` only) and chunk **10** `dthinker` SerializeThinkers.
