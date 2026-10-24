# Chunk 440 — LineSpecials[] slots 150–284 and LineSpecialTable ctor

## Scope

LineSpecials[] slots 150–284 and LineSpecialTable ctor

## Native ranges

- `src/playsim/p_lnspec.cpp` **LineSpecials[] 150–284** 3695–3833 (139 lines)
- `src/playsim/p_lnspec.cpp` **LineSpecialNames / LineSpecialTable ctor** 3835–3870 (36 lines)

**Credited native LOC:** 175 (tally `nativeLoc` 175).

## Managed

`VmPLnspecLineSpecialsTableB.cs`, `VmPLnspecLineSpecialsTableBTests.cs`
Managed LOC (tally): 109.
Conversion commit: `d19ea30086608ac2545115c639a769a509d0b1cf`.

## Tests

- `VmPLnspecLineSpecialsTableBTests.MapsToCallable_LineSetHealth`
- `VmPLnspecLineSpecialsTableBTests.IsGenericMoverSlot_CrusherDist`
- `VmPLnspecLineSpecialsTableBTests.BinarySearchStep_Positive`
- `VmPLnspecLineSpecialsTableBTests.TeleportAlias_Recognized`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 440 --to 440` (0 hard / 0 soft).

## Seams

`actionspecials.h` expansion and live `P_FindLineSpecial` binary search are not hosted.

## Credit rationale

Credits high-index dispatch slots, NOP/reserved bands, name-table resize/qsort wiring, and TeleportSpecial alias policy from `p_lnspec.cpp`.
