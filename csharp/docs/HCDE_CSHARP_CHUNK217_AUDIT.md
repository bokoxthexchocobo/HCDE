# Chunk 217 — TranslateLineDef flag translation and zero special. Retired (2026-10-23): out-of-body credit removed; honest **58** < **150**.

## Scope

TranslateLineDef flag translation and zero special. Retired (2026-10-23): out-of-body credit removed; honest **58** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `02fa8e50388075c479a56f49ef84ce2a96886447`.

## Padding review (2026-10-21)



### Out-of-body uncredit (217)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **58** |
| **Out-of-body removed** | **0** |
| **Honest** | **58** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 217 --to 217` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
