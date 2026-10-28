# Chunk 350 — DeHackEd lump filter and header sniff (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **21** < **150**.

## Scope

DeHackEd lump filter and header sniff (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **21** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmDehackedDehSuppLumpGate.cs`
Managed LOC (tally): 0.
Conversion commit: `3ff77c024b26a536fb640dc5bfaab101be85460b`.

## Padding review (2026-10-21)



### Out-of-body uncredit (350)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **21** |
| **Out-of-body removed** | **0** |
| **Honest** | **21** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 350 --to 350` (0 hard / 0 soft).

## Seams

Filesystem lump iteration and `DoDehPatch` line loop stay native; managed code encodes IWAD/PWAD filtering and v2.3 header acceptance.

## Credit rationale

Credits `isDehFile`, PWAD IWAD lump skip, and DeHackEd v2.3+ header gate only.
