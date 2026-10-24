# Chunk 268 — PatchThing post-loop finalize only (aed28a93 commit). Retired (2026-10-23): out-of-body credit removed; honest **75** < **150**.

## Scope

PatchThing post-loop finalize only (aed28a93 commit). Retired (2026-10-23): out-of-body credit removed; honest **75** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `ff3701625cad3b644630d03c193a05dea4ae1aee`.

## Padding review (2026-10-21)



### Out-of-body uncredit (268)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **75** |
| **Out-of-body removed** | **0** |
| **Honest** | **75** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 268 --to 268` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
