# Chunk 236 — ParseGameInfo GAMEINFOKEY macro expansion tail. Retired (2026-10-23): out-of-body credit removed; honest **108** < **150**.

## Scope

ParseGameInfo GAMEINFOKEY macro expansion tail. Retired (2026-10-23): out-of-body credit removed; honest **108** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `ca402cab85c470d924b7a9a8973a85b9ac582b87`.

## Padding review (2026-10-21)



### Out-of-body uncredit (236)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **109** |
| **Out-of-body removed** | **1** |
| **Honest** | **108** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 236 --to 236` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
