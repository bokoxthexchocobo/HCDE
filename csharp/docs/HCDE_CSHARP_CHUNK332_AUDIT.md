# Chunk 332 — GetDefaultLevelNum and ParseMapHeader title paths (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **23** < **150**.

## Scope

GetDefaultLevelNum and ParseMapHeader title paths (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **23** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `a4b69777e332f63b455fcc5bb03aaa91b499e944`.

## Padding review (2026-10-21)



### Out-of-body uncredit (332)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **23** |
| **Out-of-body removed** | **0** |
| **Honest** | **23** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 332 --to 332` (0 hard / 0 soft).

## Seams

`GStrings`, `HexenMusic`, and `wadlevelinfos` storage are injected or reduced to draft fields; Hexen IWAD localization is represented only by the lookup-level-name decision.

## Credit rationale

Credits MAP/E#M# default levelnum, Hexen numeric map token, `$` title prefix, and default levelnum assign — not full `ParseMapHeader` body.
