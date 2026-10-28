# Chunk 375 — LoadSegs empty-lump gate, vertex validation, split-vertex angle threshold, badseg messages; LoadSubsectors BSP sizing gates Retired (2026-10-23): out-of-body credit removed; honest **60** < **150**.

## Scope

LoadSegs empty-lump gate, vertex validation, split-vertex angle threshold, badseg messages; LoadSubsectors BSP sizing gates Retired (2026-10-23): out-of-body credit removed; honest **60** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `25221a6f2570f8964697b18892247df3d19129cd`.

## Padding review (2026-10-21)



### Out-of-body uncredit (375)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **60** |
| **Out-of-body removed** | **0** |
| **Honest** | **60** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 375 --to 375` (0 hard / 0 soft).

## Seams

Seg iteration, vertex snap geometry, linedef/sidedef binding, allocation, and subsector read loops are not hosted.

## Credit rationale

Credits only native bands exercised by named tests — not the full LoadSegs/LoadSubsectors function bodies.
