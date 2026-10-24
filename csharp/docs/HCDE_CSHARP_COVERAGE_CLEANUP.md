# Coverage cleanup pass (shrink-to-audit)

## Guardrails (enforced in tooling)

- **`shrink_chunk_credit_to_audit.py`** only **intersects** `nativeRanges` with line spans already present in `auditPaddingReview`. It **never** edits the map.
- **`--backfill-zero` / whole-range map paste** was **removed** from `apply_audit_credit_alignment.py` (wrapper errors if requested).
- After shrink: chunks with **nativeLoc &lt; 150** are **merged** into adjacent targets via `merge_chunks.py` or **retired**—never padded.
- Example honest map authorship: **chunk452** — per-method line citations derived from `VmPMapGetFriction.cs` vs `p_map.cpp` (not `650–759` wholesale).

## Totals (450–647, shrink-only; maps never auto-widened)

| Pass | Before | After | Removed |
|------|-------:|------:|--------:|
| Initial shrink (`378bbc81` + honest 641–647) | 31,015 | 27,993 | **3,022** |
| Re-shrink after post-shrink merges | 27,946 | 26,038 | **1,908** |
| Re-shrink after 573→576 / 611→615 merges | 26,038 | 25,803 | **235** |
| **Cumulative (450–647 active sum)** | **31,015** | **25,803** | **5,212** |

`convertedBehaviorNative` after `sync_loc_tally.py`: see `loc_tally.json` (shrink lowers per-chunk `nativeLoc`; tally follows chunk JSON).

Machine-readable:
- **Applied deltas vs `378bbc81`:** `csharp/docs/HCDE_CSHARP_COVERAGE_SHRINK_APPLIED.json` (per-chunk before/after + cumulative removed).
- **Last dry-run:** `csharp/docs/HCDE_CSHARP_COVERAGE_SHRINK_REPORT.json` (idempotent check; `removed` should be 0 after a clean apply).

## Post-shrink merges (nativeLoc floor)

| Target | Sources merged |
|--------|----------------|
| 601 | 602, 604 |
| 616 | 603 |
| 615 | 610, 614, 611 |
| 611 | 612, 613 |
| 618 | 617 |
| 622 | 619, 620, 623 |
| 644 | 645 |
| 550 | 549 |
| 552 | 551 |
| 547 | 545 |
| 566 | 565 |
| 567 | 568 |
| 570 | 569 |
| 573 | 572 |
| 576 | 573, 578 |
| 593 | 592 |
| 636 | 635 |
| 640 | 639 |
| 453 | 452 (honest friction predicates) |

## Remaining work

- Playsim/codegen chunks still on **“see native citation”** audits: add **statement-level** map rows from code review, then re-run shrink.
- Chunks above **400 nativeLoc** after merge may need split (reaudit `nativeLoc_above_ceiling`).
