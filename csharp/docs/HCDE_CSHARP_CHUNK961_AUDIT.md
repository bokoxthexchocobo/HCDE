# Chunk 961 — Map CVAR defaults, `P_ShouldPassThroughPlayer`, and `P_CanCollideWith` virtual head from `p_map.cpp` (**1–150**).

## Scope

Map CVAR defaults, `P_ShouldPassThroughPlayer`, and `P_CanCollideWith` virtual head from `p_map.cpp` (**1–150**).

## Native ranges

- `src/playsim/p_map.cpp` **P_ShouldPassThroughPlayer** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMapFileHead961Slice.cs`
Managed LOC (tally): 83.
Conversion commit: `34bd514eb300e41cdae7e2c83f402d52e089df66`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Client/server CVAR defaults (**68–71**) | yes |
| `P_ShouldPassThroughPlayer` (**98–103**) | yes |
| `AActor::ShouldPassThroughPlayer` export (**105–110**) | seam (VM export) |
| `P_CanCollideWith` virtual dispatch head (**120–150**) | yes |
| Static protos / `FRandom` (**73–80**) | seam |

| Managed (`VmPMapFileHead961Slice.cs`) | Native (`p_map.cpp`) |
|---------------------------------------|---------------------|
| `NoPlayerClipEnabled` / `ShouldPassThroughPlayer` | **100–103** |
| `ResolveCanCollideVirtualIndex` / table bounds | **122–136** |
| `VmCollideCallBlocked` / `CollidePairAllowed` / mate pass | **139–150** |
| `AppendSpechitWhenNeeded` | **84–85** deferral policy (special vs portal) |
| CVAR default constants | **68–71** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **961** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**960** |


**Post-review ratio:** 83/150 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapFileHead961SliceTests.ShouldPassThroughPlayer_RequiresFlagFriendAndPlayerMo`
- `VmPMapFileHead961SliceTests.ResolveCanCollideVirtualIndex_CachesAfterFirstResolve`
- `VmPMapFileHead961SliceTests.ActorVirtualTableLargeEnough_MatchesNativeBoundsCheck`
- `VmPMapFileHead961SliceTests.CollidePairAllowed_BothVirtualCallsMustPass`
- `VmPMapFileHead961SliceTests.AppendSpechitWhenNeeded_RecordsPortalAndSpecialSeparately`
- `VmPMapFileHead961SliceTests.EvaluateSecondCanCollideVirtual_RequiresMateParam`
- `VmPMapFileHead961SliceTests.EvaluateFirstCanCollideVirtual_BlocksWhenVmReturnsZero`
- `VmPMapFileHead961SliceTests.DefaultCvarPolicy_MatchesNativeArchiveDefaults`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 961 --to 961` (0 hard / 0 soft).

## Seams

Includes, `FRandom` streams, `spechit`/`portalhit` globals, VM export wrapper, and `P_CollidedWith` tail remain host seams.

## Credit rationale

Honest **150**-line UNCredited band; exclusive `VmPMapFileHead961Slice.cs` (before later credited map bands **704+**).
