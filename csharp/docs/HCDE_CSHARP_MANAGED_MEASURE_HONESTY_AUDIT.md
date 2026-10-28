# Managed LOC measurement honesty audit (2026-10-20)

## Problem

`managedLoc` and land-gate coverage used **summed `git show --numstat` additions** across `conversionManagedCommit` and `portCommits`. Lines that were added for ratio padding and later deleted still counted (e.g. chunk **524** credited **362** while only **206** lines remain in `VmAcsDoSetActorProperty.cs` at HEAD).

## Current rules (HEAD surviving credit)

| Rule | Implementation |
|------|----------------|
| **(a) Exclusive attribution** | Each `csharp/src/*.cs` path appears in **at most one** `chunk{N}.json` `managedSourceFiles` entry. `ManagedSourceFilesAreExclusiveAcrossChunkDefinitions` enforces this. No line is double-counted across chunks. |
| **(b) Src only** | Only paths under `csharp/src/` are allowed. `csharp/tests/**` is **never** credited in `managedLoc` (tests are listed separately in chunk audits). |
| **(c) Line counting** | **Physical lines** in attributed src files at repo HEAD (same spirit as historical `git numstat` additions: includes `///` doc lines and braces; excludes nothing inside the file). Blank-only lines inside the file are included so land commits and HEAD counts stay aligned. Stricter non-blank-only counting was tried and produced false **thin_managed_coverage** on doc-heavy VMs (e.g. 522–523); physical HEAD lines are the gate. |

Measurement entry points: `managed_loc_surviving_at_head()` in `chunk_port_managed.py`, `measure_chunk_managed_loc.py`, `check_port_coverage.sh`, `reaudit_chunks.py`, and `LocTallyManagedLocMatchesSurvivingSourceAtHead`.

Land gate: `check_port_coverage.sh` fails if surviving HEAD LOC `< 50%` of credited native, or if `conversionManagedCommit` numstat additions **>** surviving HEAD (padding removed after commit).

`conversionManagedCommit` on remeasure is refreshed to `git log -1` touching each attributed file (audit trail only; **not** added to managed credit).

## Why `convertedBehaviorManaged` went up (e.g. 59,608 → 60,882 at HEAD)


Honest HEAD counting **increased** credit for chunks that were **under-counted** by commit-addition sums when VM files grew after the land commit (e.g. **507** commit credit 88 vs **116** HEAD lines; **455**/**463** whole-file growth). It **decreased** credit where padding was removed (**524** 362 → **206**) or where commit sums overstated surviving files. Net session band (450–527 remeasure) moved up because several large VMs were previously credited below their current exclusive file size.

## Chunk 524 (DoSetActorProperty)

| | Lines |
|---|------|
| **Src** (`VmAcsDoSetActorProperty.cs`, credited) | **206** |
| **Tests** (`VmAcsDoSetActorPropertyTests.cs`, not in `managedLoc`) | **73** |
| Native credited | 241 |
| Managed / native | **85.5%** |

Padding removal and HEAD remeasure documented in `HCDE_CSHARP_CHUNK524_AUDIT.md`.

## Session remeasure (450–527)

`reaudit_chunks.py audit --from 450 --to 527`: **0 hard / 0 soft** after HEAD physical remeasure and stale-commit soft removal.

## “Deepen” padding review (502, 496, 469, 500, 509, 487)

No stub `=> true` padding pattern found in attributed VMs (see prior table in this file’s git history).

## Cumulative totals

See `loc_tally.json` headers (`convertedBehaviorManaged` sums tally `managedLoc` for conversion chunks).
