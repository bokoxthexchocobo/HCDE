# Chunk 440 — LineSpecials[] slots 150–284 and LineSpecialTable ctor Retired (2026-10-23): out-of-body credit removed; honest **25** < **150**.

## Scope

LineSpecials[] slots 150–284 and LineSpecialTable ctor Retired (2026-10-23): out-of-body credit removed; honest **25** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `8346d924af0f51b3e1eb52bf21b57df284865980`.

## Padding review (2026-10-21)



### Out-of-body uncredit (440)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **175** |
| **Out-of-body removed** | **150** |
| **Honest** | **25** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 440 --to 440` (0 hard / 0 soft).

## Seams

`actionspecials.h` expansion and live `P_FindLineSpecial` binary search are not hosted.

## Credit rationale

Credits high-index dispatch slots, NOP/reserved bands, name-table resize/qsort wiring, and TeleportSpecial alias policy from `p_lnspec.cpp`.
