# Chunk 356 — AM_ParseArrow lump parser and AM_StaticInit mark textures Retired (2026-10-23): out-of-body credit removed; honest **52** < **150**.

## Scope

AM_ParseArrow lump parser and AM_StaticInit mark textures Retired (2026-10-23): out-of-body credit removed; honest **52** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `3dd3627b827db7d540b764f9885a1e1eb10911c0`.

## Padding review (2026-10-21)



### Out-of-body uncredit (356)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **57** |
| **Out-of-body removed** | **5** |
| **Honest** | **52** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 356 --to 356` (0 hard / 0 soft).

## Seams

`fileSystem` lump IO and `TexMan` lookups are callbacks; vector arrays mirror `mline_t` scaling.

## Credit rationale

Credits default thin triangle, `AM_ParseArrow` coordinate scaling, fatal guard when no arrow, and `AMMNUM` mark lump names.
