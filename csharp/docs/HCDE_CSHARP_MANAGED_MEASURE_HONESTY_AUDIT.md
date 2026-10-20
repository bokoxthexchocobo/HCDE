# Managed LOC measurement honesty audit (2026-10-20)

## Problem

`managedLoc` and land-gate coverage used **summed `git show --numstat` additions** across `conversionManagedCommit` and `portCommits`. Lines that were added for ratio padding and later deleted still counted (e.g. chunk **524** credited **362** while only **206** lines remain in `VmAcsDoSetActorProperty.cs` at HEAD).

## Tooling fix (hard gates unchanged elsewhere)

| Component | Change |
|-----------|--------|
| `chunk_port_managed.py` | **`managed_loc_surviving_at_head`**: physical line count of `managedSourceFiles` at repo HEAD. `managed_loc_for_chunk` now aliases this. |
| `check_port_coverage.sh` | Uses surviving HEAD LOC; **hard-fails** if `conversionManagedCommit` numstat additions **>** surviving (deleted-line inflation on land). |
| `measure_chunk_managed_loc.py` | Rewrites tally `managedLoc` from surviving HEAD for rows with `managedSourceFiles` (chunk JSON or tally). |
| `reaudit_chunks.py` | **Hard-fail** `managedLoc_mismatch` when tally ≠ surviving; **soft-warn** `managed_loc_stale_commit_additions` when commit additions > surviving (stale SHA after edits). |
| `LocTallyIntegrityTests` | New **`LocTallyManagedLocMatchesSurvivingSourceAtHead`**. |

`portCommits` no longer affect measurement. Remaining `portCommits` keys were removed from chunk JSON files.

## Chunk 524 (DoSetActorProperty)

- Padding removed earlier; surviving managed **206** / native **241** (**85.5%**).
- Tally corrected **362 → 206** (no deleted padding credit).

## Session remeasure (chunks 450–527)

Twenty tally rows changed when switching to HEAD surviving counts. None fell below **50%** managed/native or below **150** native.

| Chunk | Native | Old managedLoc | New managedLoc | Ratio |
|-------|--------|----------------|----------------|-------|
| 454 | 168 | 140 | 139 | 0.83 |
| 455 | 289 | 194 | 396 | 1.37 |
| 461 | 170 | 107 | 221 | 1.30 |
| 462 | 199 | 104 | 198 | 1.00 |
| 463 | 151 | 114 | 336 | 2.23 |
| 464 | 160 | 113 | 129 | 0.81 |
| 465 | 159 | 108 | 138 | 0.87 |
| 469 | 158 | 163 | 182 | 1.15 |
| 470 | 190 | 104 | 131 | 0.69 |
| 475 | 249 | 176 | 265 | 1.06 |
| 478 | 185 | 234 | 209 | 1.13 |
| 491 | 173 | 120 | 118 | 0.68 |
| 496 | 162 | 137 | 153 | 0.94 |
| 502 | 229 | 224 | 215 | 0.94 |
| 503 | 195 | 215 | 212 | 1.09 |
| 506 | 194 | 110 | 109 | 0.56 |
| 507 | 157 | 88 | 116 | 0.74 |
| 513 | 194 | 108 | 113 | 0.58 |
| 518 | 246 | 139 | 143 | 0.58 |
| 524 | 241 | 197 | 206 | 0.86 |

Ratios above 1.0 mean the VM file grew after the initial land (shared-file growth or deepen); credit is still **only current file lines**, not historical commits.

`reaudit_chunks.py audit --from 450 --to 527`: **0 hard / 1 soft** (`managed_loc_stale_commit_additions` on chunk 503).

## “Deepen” padding review (502, 496, 469, 500, 509, 487)

Manual scan of attributed VMs for stub `=> true`, duplicate `value != 0` helpers, always-true seams, and thin wrappers:

| Chunk | VM(s) | Finding |
|-------|-------|---------|
| 502 | `VmPMobjTagDamageRestore`, `VmPMobjPrintMiscActorInfo` | No padding pattern; tests exercise tag/damage/restore/print paths. |
| 496 | `VmPMapSectorPush` | No padding pattern. |
| 469 | `VmPEnemyLookEx` | No padding pattern. |
| 500 | `VmPMobjTeamFriendDamage` | No padding pattern. |
| 509 | `VmPUserPredictClient` | No padding pattern. |
| 487 | `VmPMobjSpawnMapThingBands` | No padding pattern. |

## Cumulative totals (after sync)

See `loc_tally.json` headers after this commit (`convertedBehaviorManaged` reflects surviving-head sums).

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 450 --to 527` (0 hard).
