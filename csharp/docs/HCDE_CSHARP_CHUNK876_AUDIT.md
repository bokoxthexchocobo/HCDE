# Chunk 876 — Retired (2026-10-23): uncredited separator/header/comment ranges removed per policy; honest whole-function credit **115** (< **150** floor). Prior tally **169** included banned separator/banner credit. Matrix credit **0**; `VmAAction.cs` remains off-tally.

## Scope

Retired (2026-10-23): uncredited separator/header/comment ranges removed per policy; honest whole-function credit **115** (< **150** floor). Prior tally **169** included banned separator/banner credit. Matrix credit **0**; `VmAAction.cs` remains off-tally.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `4b7c984bbdf35a785fb22db377c5aa7079e4a2ac`.

## Padding review (2026-10-21)

### Incident — separator/header credit (corrected)

| | `nativeLoc` |
|--|----------:|
| **Before** (tally, incl. headers **1–36**, separators, comment blocks) | **169** |
| **After uncredit** (only `HCDE_QueueCorpse`, `A_Unblock`, `A_QueueCorpse`, `A_DeQueueCorpse`) | **115** |
| **After retire** (matrix) | **0** |

### Removed from credit (never behavior-ported)

| Native lines | Reason |
|--------------|--------|
| **1–36** | headers / includes |
| **80–86**, **137**, **144–145**, **156–157** | separators |
| **138–143** | comment banner |

### Behavior ported (matrix credit)

| Native | Behavior ported (off-tally) |
|--------|------------------------------|
| `HCDE_QueueCorpse` / `A_Unblock` / `A_QueueCorpse` / `A_DeQueueCorpse` | yes in `VmAAction.cs` |
| Matrix credit | **no** (retired) |

### Gross vs net

| | Value |
|--|------:|
| Gross (invalid tally) | **169** |
| Honest functions only | **115** |
| Net behavior-ported (matrix) | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 876 --to 876` (0 hard / 0 soft).

## Seams

`P_DropItem` / `PClass::FindActor` supplied at unblock sites; corpse list is host-owned `IList<Actor>`.

## Credit rationale

Separator/header padding (chunk **876** pattern cited in invalid **893** top-up) must not count toward `nativeLoc`. After uncredit, band is below floor → retire.
