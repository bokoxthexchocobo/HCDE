# Chunk 326 — EMAPINFO SetLevelNum GetOrCreateLevel NormalizeMusicName stats apply bool Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

EMAPINFO SetLevelNum GetOrCreateLevel NormalizeMusicName stats apply bool Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmMapInfo*.cs` (see conversion commit)
Managed LOC (tally): 0.
Conversion commit: `c9485c42a4e05d5e5e0ae462d71211d67b115356`.

## Padding review (2026-10-21)



### Out-of-body uncredit (326)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **92** |
| **Out-of-body removed** | **92** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 326 --to 326` (0 hard / 0 soft).

## Seams

`FindLevelInfo` and `wadlevelinfos` storage are modeled as injectable lists; `RecordUnsupported`/`ApplyBoolProperty` reuse existing managed stats helpers without pulling in the full parse loop.

## Credit rationale

Covers the uncredited `hcde_emapinfo.cpp` band 302–394 between chunk 283's `GetDefaultLevelNum` tail and chunk 284's `GetOrCreateLevel` row: duplicate `levelnum` clearing, lazy level creation with default MAP/E#M# numbering, Doom/Heretic music prefix normalization, and the stats/bool-apply helpers that feed the property dispatcher.
