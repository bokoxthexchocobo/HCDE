# Chunk 579 — `RadiusGiveFlags`, `DoRadiusGive` eligibility/range gates, and `A_RadiusGive` validation/loops (`p_actionfunctions.cpp` **3774–3985**). Replaces legacy `action-explode-radius-give` row; `VmActionRadiusGiveEligibility` stays on **577** (species/class only).

## Scope

`RadiusGiveFlags`, `DoRadiusGive` eligibility/range gates, and `A_RadiusGive` validation/loops (`p_actionfunctions.cpp` **3774–3985**). Replaces legacy `action-explode-radius-give` row; `VmActionRadiusGiveEligibility` stays on **577** (species/class only).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **RadiusGiveFlags** 3774–3805 (32 lines)
- `src/playsim/p_actionfunctions.cpp` **DoRadiusGive** 3810–3857 (48 lines)
- `src/playsim/p_actionfunctions.cpp` **DoRadiusGive** 3859–3867 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **DoRadiusGive** 3869–3888 (20 lines)
- `src/playsim/p_actionfunctions.cpp` **DoRadiusGive** 3891–3912 (22 lines)
- `src/playsim/p_actionfunctions.cpp` **A_RadiusGive** 3953–3962 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **A_RadiusGive** 3965–3983 (19 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmActionDoRadiusGive.cs`
Managed LOC (tally): 127.
Conversion commit: `41fc05c3582cf4902b8b4cd8d8af91f7f49e44db`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `RgfMask` | `RadiusGiveFlags` **3774–3805** |
| `TryPassSelf` / `TryPassMissile` / `TryPassItemCorpseOrKilled` / `TryPassLivingActor` | `DoRadiusGive` **3810–3853** |
| `FilterSpeciesGate` | **3859–3867** |
| `PointerRelationGate` / `TargetPass` / `MasterPass` / `TracerPass` | **3869–3888** |
| `RangeCheckHeightOffset` / `OutsideRange` | **3891–3912** (`P_CheckSight`/`Spawn` seam) |
| `RadiusGiveAbortsEarly` / `NormalizeGiveAmount` / `UnlimitedGiveLimit` / `ShouldIterateMissiles` | `A_RadiusGive` **3953–3983** (iterator bodies seam) |

### vs legacy `action-explode-radius-give`

| | Lines |
|--|------:|
| Legacy row (retired on land) | **3807–3985** (thin map test) |
| **579** (this row) | **3774–3985** (seam-trimmed) |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **162** |
| Net behavior-ported | **162** |


**Post-review ratio:** 127/160 managed/native ≈ 79% (src file LOC at HEAD; see tally).

## Tests

- `VmActionDoRadiusGiveTests.TryPassMissile_WhenFlagged`
- `VmActionDoRadiusGiveTests.TryPassSelf_RequiresFlag`
- `VmActionDoRadiusGiveTests.TryPassLivingActor_PlayerBranch`
- `VmActionDoRadiusGiveTests.FilterSpeciesGate_AllowsSelfDespiteFilter`
- `VmActionDoRadiusGiveTests.PointerRelationGate_BlocksNotargetHit`
- `VmActionDoRadiusGiveTests.OutsideRange_UsesCubeWhenFlagged`
- `VmActionDoRadiusGiveTests.RadiusGiveAbortsEarly_WhenDistanceZero`
- `VmActionDoRadiusGiveTests.NormalizeGiveAmount_DefaultsToOne`
- `VmActionDoRadiusGiveTests.ShouldIterateMissiles_WhenFlagged`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 579 --to 579` (0 hard / 0 soft).

## Seams

`Spawn`, health amount scaling, `CallTryPickup`, `P_CheckSight`, thinker/blockmap iterators, and `TranslateLineDef` are not hosted.

## Credit rationale

Honest re-land of radius-give policy: flag mask, per-thing pass branches, filter/species/pointer gating, cube/sphere distance checks, and give abort/limit/amount setup.
