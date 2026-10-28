# Chunk 962 — Retired (2026-10-28): attempted band **872–1021** overlaps existing managed `VmPMobjSetStateUse.cs` (**856–1095**, chunk **737** retire row). Standing rule: no duplicate credit when managed already ports `SetState` / `DestroyAllInventory` / `UseInventory` head — matrix credit **0**.

## Scope

Retired (2026-10-28): attempted band **872–1021** overlaps existing managed `VmPMobjSetStateUse.cs` (**856–1095**, chunk **737** retire row). Standing rule: no duplicate credit when managed already ports `SetState` / `DestroyAllInventory` / `UseInventory` head — matrix credit **0**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f50b7bca8a0547578a40004be7805ae4065cbff4`.

## Padding review (2026-10-21)

### Retire reason (**962**)

| Issue | Resolution |
|-------|------------|
| `VmPMobjSetStateUse.cs` already ports **856–961**, **978–1005**, **1024+** stubs | Abort matrix credit on **872–1021** |
| `VmPMobjSetStateInventory962Slice` | Deleted — exclusive helper re-port = double-count |
| Chunk **737** | Already retired for **738** overlap; managed file remains canonical off-tally port |

### Behavior ported (matrix credit)

| Native | Behavior ported |
|--------|-----------------|
| (none) | **no** |

### Gross vs net

| | Value |
|--|------:|
| Gross attempted (invalid land) | **150** |
| Net behavior-ported (matrix) | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 962 --to 962` (0 hard / 0 soft).

## Seams

N/A — row retired before valid exclusive port.

## Credit rationale

`VmPMobjSetStateUse.SetState` mirrors `AActor::SetState` loop; `DestroyAllInventory` / `UseInventory` helpers already tested off-tally. `VmPMobjSetStateInventory962Slice` was redundant helper duplication — removed.
