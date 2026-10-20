# Re-audit managed gate — strict commit measure (2026-10-20)

## Change

Reverted a temporary relaxation in `reaudit_chunks.py` that set `thin_managed_coverage` using `max(commit_ml, tally_ml, managedSourceFiles lines)`.

## Reason

Consolidated tally row **d-doommain-restart-loop-chunk445** credits five port commits’ native bands but pointed at a single `conversionManagedCommit`. Inflating `measured` from tally or `chunk*.json` hid a real **thin_managed_coverage** soft warning instead of fixing credit or port depth.

## Rule (unchanged)

`thin_managed_coverage` uses **only** `csharp/src/*.cs` additions from `conversionManagedCommit` (`commit_ml`), falling back to tally `managedLoc` when the commit has zero measured additions. Gate tooling must not widen this check.

## Fix path for chunk 445

New port commit expanding the five credited `VmDDoomMain*` / `VmDMain*` files so `commit_ml ≥ 50%` of credited `nativeLoc` (218), then point `conversionManagedCommit` at that commit.

`measure_chunk_managed_loc.py` `managedSourceFiles` remains a **tally headline** helper only; it does not affect re-audit.

`check_port_coverage.sh` accepts an optional second argument: the **port commit** SHA (defaults to `HEAD`). Run it against the port commit when the tally commit is separate.
