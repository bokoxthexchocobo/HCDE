# Chunk 242 — SaveStatistics and time format helpers. Retired (2026-10-23): out-of-body credit removed; honest **136** < **150**.

## Scope

SaveStatistics and time format helpers. Retired (2026-10-23): out-of-body credit removed; honest **136** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `356603fb17391e452abd53d02a17ebd5a09ae3df`.

## Padding review (2026-10-21)



### Out-of-body uncredit (242)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **139** |
| **Out-of-body removed** | **3** |
| **Honest** | **136** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 242 --to 242` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
