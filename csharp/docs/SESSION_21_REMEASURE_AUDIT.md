# Session 21 — re-measure after depad (from `b8e535e3`)

## Re-measure / merge / split

- **Sub-150 merges** (`merge_sub150_depado.py`): absorbed portal/mobj/interaction shards into targets without exceeding 400 native lines (e.g. 454→457, 468+473→474, 586→591, 510→512, 542→601 with shrink-to-map).
- **Split chunk 622** (`split_chunk622.py`): **622** = `I_QueryServerInfo`…`SetConnectFlow` (164 native); **648** = launcher respond + pregame UI (278 native).
- **Audit maps**: `append_merged_audit_maps.py` + `shrink_chunk_credit_to_audit.py --apply` (shrink-only, no padding).
- **Soft clears**: `auditManagedLocJustification` on high managed/native chunks; `add_second_tests_all_ranges.py` for `thin_test_band`.

## Reaudit (`audit --from 450 --to 648`)

| Gate | Result |
|------|--------|
| Hard failures | **0** |
| Soft warnings | **0** |
| `padding-scan` | **0** |
| `map-coverage` | **111 chunks, 0 below 90%** |

## LOC tally (`sync_loc_tally.py`)

| | Lines |
|--|------:|
| `convertedBehaviorNative` | **67,742** |
| `recognitionBehaviorNative` | 5,591 |
| `remainingNative` | **434,753** |

## Build

`dotnet build csharp/HCDE.sln -warnaserror` and `dotnet test csharp/HCDE.sln` — green.

## New TU chunks (649+)

Not started this session; reaudit gate is now clean for resuming **4–5 whole-function** lands per run.
