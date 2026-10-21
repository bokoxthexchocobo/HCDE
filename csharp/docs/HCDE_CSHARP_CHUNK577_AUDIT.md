# Chunk 577 — RS_Flags and A_Respawn success path (2132–2281)

## Scope

RS_Flags and A_Respawn success path (2132–2281)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **RS_Flags** 2132–2137 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **A_Respawn** 2139–2288 (150 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmActionRespawn.cs`
Managed LOC (tally): 79.
Conversion commit: `1c51248582425e443a294f1933e91560ef133672`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `UseTelefragMove` / `RespawnSucceeded` | `A_Respawn` 2152–2162 |
| `ShouldClearTarget` / `ShouldClearSelfTarget` | `A_Respawn` 2169–2181 |
| `MergeFriendlyFlags` / `RestoreDefaultHeight` / `RestoreDefaultRadius` | `A_Respawn` 2147–2150, 2184–2194 |
| `ShouldSpawnFog` | `A_Respawn` 2196–2200 |

**Post-review ratio:** 79/156 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmActionRespawnTests.UseTelefragMove_WhenFlagSet`
- `VmActionRespawnTests.ShouldSpawnFog_WhenFlagSet`
- `VmActionRespawnTests.ShouldClearTarget_WhenKeepTargetUnset`
- `VmActionRespawnTests.ShouldClearSelfTarget_WhenSameReference`
- `VmActionRespawnTests.MergeFriendlyFlags_PreservesFriendlyBit`
- `VmActionRespawnTests.RestoreDefaultHeight_Passthrough`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 577 --to 577` (0 hard / 0 soft).

## Seams

`P_TeleportMove`, `P_CheckPosition`, `P_SpawnTeleportFog`, and monster counters are not hosted.

## Credit rationale

Faithful managed mirrors for respawn flag handling, target cleanup, and friendly-flag merge.
