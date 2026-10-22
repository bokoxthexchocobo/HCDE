# Chunk 809 — Whole `FTagManager` mutation/query APIs and `FSectorTagIterator` / `FLineIdIterator` from `p_tags.cpp` (36–393 except `DumpTags`, 404–464).

## Scope

Whole `FTagManager` mutation/query APIs and `FSectorTagIterator` / `FLineIdIterator` from `p_tags.cpp` (36–393 except `DumpTags`, 404–464).

## Native ranges

- `src/playsim/p_tags.cpp` **FTagManager::AddSectorTag** 36–63 (28 lines)
- `src/playsim/p_tags.cpp` **FTagManager::RemoveSectorTags** 71–85 (15 lines)
- `src/playsim/p_tags.cpp` **FTagManager::RemoveLineIDs** 93–107 (15 lines)
- `src/playsim/p_tags.cpp` **FTagManager::AddLineID** 115–141 (27 lines)
- `src/playsim/p_tags.cpp` **FTagManager::HashTags** 149–181 (33 lines)
- `src/playsim/p_tags.cpp` **FTagManager sector/line queries** 189–285 (97 lines)
- `src/playsim/p_tags.cpp` **FTagManager::CountSectorTags** 311–348 (38 lines)
- `src/playsim/p_tags.cpp` **FTagManager::CountLineIDs** 356–393 (38 lines)
- `src/playsim/p_tags.cpp` **FSectorTagIterator::Next** 404–431 (28 lines)
- `src/playsim/p_tags.cpp` **FSectorTagIterator::NextCompat** 439–448 (10 lines)
- `src/playsim/p_tags.cpp` **FLineIdIterator::Next** 457–464 (8 lines)

**Credited native LOC:** 337 (tally `nativeLoc` 337).

## Managed

`VmPTagManager.cs`
Managed LOC (tally): 298.
Conversion commit: `5a5b7f83` (port). Tally commit: `ec0c40f9`.

## Padding review (2026-10-21)

| Managed | Native (`p_tags.cpp`) |
|---------|----------------------|
| `AddSectorTag` | **36–63** |
| `RemoveSectorTags` | **71–85** |
| `RemoveLineIds` | **93–107** |
| `AddLineId` | **115–141** |
| `HashTags` | **149–181** |
| `SectorHasTags` / queries | **189–285** |
| `CountSectorTags` / `GetSectorTag` | **311–348** |
| `CountLineIds` / `GetLineId` | **356–393** |
| `SectorTagIterator::Next` | **404–431** |
| `NextCompat` | **439–448** |
| `LineIdIterator::Next` | **457–464** |

**Post-review ratio:** 298/337 managed/native ≈ 88% (src file LOC at HEAD; see tally).

## Tests

- `VmPTagManagerTests.SectorHasTag_DetectsDuplicateTags`
- `VmPTagManagerTests.LineIdIterator_WalksHashChain`
- `VmPTagManagerTests.SectorTagIterator_FindsTaggedSectors`
- `VmPTagManagerTests.LineHasId_FindsMatchingId`
- `VmPTagManagerTests.GetSectorTag_ByIndex`

## Uncredited-range check (pre-port)

```
uncredited_range_check.py --chunk 809
  [UNCredited] FTagManager::AddSectorTag … through FLineIdIterator::Next (all bands)
exit 0
```

## Portfolio delta

| | Native LOC |
|---|---:|
| **Gross** (`p-tags-manager-chunk809`) | **+337** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+337** (93,041 → **93,378**) |

## Land hygiene (amend / origin / portal tests)

**Git amend (local only):** During 809 repair the port commit was amended several times on the workstation (`f44dcb1e` → `2bebd7cf` → `c895aca1` → `5a5b7f83`). Reflog also shows a discarded `git reset --soft HEAD~2` and an un-pushed `58604a82` (“TagItem field order”) that was folded into the final port tree. **None of the amended SHAs (`f44dcb1e`, `2bebd7cf`, `c895aca1`, `8826cc58`, `58604a82`) appear on `origin`.** The only 809 objects pushed were **`5a5b7f83`** (port) and **`ec0c40f9`** (tally), atop **`6729613f`**. Policy: **no further amends**; use new commits only.

**Portal test overwrite:** While consolidating `LineIdIterator` into `VmPTagManager`, `VmPortalBuildBlockmapTests.cs` and `VmPortalChangePortalTests.cs` were briefly replaced with **minimal** versions (~26 and ~25 lines) via a full-file write in the agent session. Before push they were **restored from `6729613f`** (`git show 6729613f:…`) and updated only for `VmPTagManager.LineIdIterator` / `TagItem(target, tag, nextTag)` field order. **HEAD vs `6729613f`:** both files are **89 and 33 lines** (unchanged line count); diff is iterator type/entry field fixes only — all `[Fact]` methods from the pre-809 full files are present (`AddLineIntercepts_PushesCrossingLine`, `FindPortalDestination_SkipsSourceLine`, `SetPortalRotation_*`, `ChangePortalLine_RejectsLinked`, `ChangePortal_AdvancesLineIdIterator`).

**Playsim test count:** `HCDE.Playsim.Tests` **18,849** tests at **`6729613f`** vs **18,834** at **`ec0c40f9`** (−15). The delta is **not** from portal files; it is from **intentional removal** of `VmPSpecFrictionSectorTests` (8 facts) and `VmPSpecSectorDamageTests` (11 facts) when deleting superseded VMs, partially offset by new `VmPTagManagerTests` / `VmPSpecShowSecretCvarsTests` / consolidated specs. `VmPLnspecFloorCeilingTests` was trimmed during 809 and **restored to five facts** in a follow-up commit.

## Tally correction (chunk **685**, same land sync)

`portal-blockmap-update-chunk685` **`managedLoc` 364 → 336** at HEAD, matching surviving `VmPortalBuildBlockmap.cs` + `VmPortalUpdateFinalize.cs` after moving `LineIdIterator` out of `VmPortalBuildBlockmap` (reaudit `managedLoc_mismatch` fix during 809 tally; no native credit change).

## VM consolidation test retarget (post-809)

Deleted `VmPSpecFrictionSectorTests` / `VmPSpecSectorDamageTests` **19** `[Fact]` methods retargeted at HEAD:

| Former test | New home | Notes |
|-------------|----------|-------|
| Friction 8× | `VmPSpecActorInSpecialSectorTests` | `IronFeetProtectionLevel_FullWins` / leak tests exercise `P_ActorInSpecialSector` iron-feet branch (`p_spec.cpp` 406–501), not standalone helpers. |
| Damage 11× | `VmPSpecSectorDamageRunTests` (+ `ActorOnSecretSector` on actor TU) | `OrInAirFlag` is `flags \| DamageInAir` constant fold only (old chunk-442 shim); `VavoomSwapHeights_Swaps` mirrors swap in `P_SectorDamage` 3D-floor loop. |
| `PrintSecretSector` | `VmPSpecSectorDamageRunTests` + `VmPSpecShowSecretCvarsTests` | Cvar band **593–594** still on chunk **442**. |

`HCDE.Playsim.Tests` **18,852** ≥ **18,849** pre-809.

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `bash csharp/tools/verify_full_solution_tests.sh` after tally; `python3 tools/reaudit_chunks.py audit --from 450 --to 809` (**0 hard / 0 soft**); `python3 tools/reaudit_chunks.py padding-scan --from 450 --to 809` (**0** unallowlisted hits).

## Seams

Sector/line `Index()` accessors and `Printf` diagnostics are injected; `DumpTags` left uncredited.

## Credit rationale

First tally credit for `p_tags.cpp`; consolidates duplicate `LineIdIterator` helper off `VmPortalBuildBlockmap`.
