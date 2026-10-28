# Chunk 928 — `LS_Sector_SetLink` through `LS_Sector_SetGravity` (`p_lnspec.cpp` **2269–2418**). Pivot from deferred `p_acs` tail; follows **903** gap (**1481–2268** uncredited); `LS_Sector_SetColor` at **2421** deferred.

## Scope

`LS_Sector_SetLink` through `LS_Sector_SetGravity` (`p_lnspec.cpp` **2269–2418**). Pivot from deferred `p_acs` tail; follows **903** gap (**1481–2268** uncredited); `LS_Sector_SetColor` at **2421** deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Sector_SetLink** 2269–2418 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecScrollSector928Slice.cs`
Managed LOC (tally): 107.
Conversion commit: `a203ab98f73167f56ddbdabfe0c0a54635f7e741`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| `SectorSetLink*` | `LS_Sector_SetLink` **2269–2281** |
| `ScrollTextureBoth` / `NormalizeTextureScrollId` | **2286–2309** |
| `ScrollWall` | **2311–2319** |
| `ScrollFloor*` / `ScrollFloorDeltas` | **2325–2348** |
| `ScrollCeiling` | **2350–2358** |
| `PointPushAlwaysFails` | **2360–2364** |
| `NormalizeLegacyLeakiness` / `ApplyDamageMod` | `LS_Sector_SetDamage` **2366–2402** |
| `ClampGravityFraction` / `SectorGravity` | `LS_Sector_SetGravity` **2404–2419** |

### Chunk **928** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**927** |


**Post-review ratio:** 107/150 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecScrollSector928SliceTests.SectorLink_SkipsControlTagZero`
- `VmPLnspecScrollSector928SliceTests.ScrollTextureBoth_NormalizesNegativeId`
- `VmPLnspecScrollSector928SliceTests.Damage_LegacyLeakinessForLowAmount`
- `VmPLnspecScrollSector928SliceTests.Gravity_ClampsFraction`
- `VmPLnspecScrollSector928SliceTests.ScrollFloor_SplitsCarryAndScroller`
- `VmPLnspecScrollSector928SliceTests.PointPush_AlwaysFails`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 928 --to 928` (0 hard / 0 soft).

## Seams

`P_AddSectorLinks`, `SetWallScroller`, `SetScroller`, sector tag iterators, and `MODtoDamageType` application remain host seams.

## Credit rationale

Honest **150**-line head slice of **2269–3129** corridor; exclusive `VmPLnspecScrollSector928Slice.cs`.
