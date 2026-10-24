# Chunk 241 — Statistics structs and ParseStatistics. Retired (2026-10-23): out-of-body credit removed; honest **64** < **150**.

## Scope

Statistics structs and ParseStatistics. Retired (2026-10-23): out-of-body credit removed; honest **64** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `82281d744ad768fd2694173b871e4c23ad3e46a5`.

## Padding review (2026-10-21)



### Out-of-body uncredit (241)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **156** |
| **Out-of-body removed** | **92** |
| **Honest** | **64** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 241 --to 241` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
