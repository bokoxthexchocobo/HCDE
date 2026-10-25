# Chunk 444 — P_GetMaxLineSpecial, P_FindLineSpecial binary search, P_ExecuteSpecial Retired (2026-10-23): out-of-body credit removed; honest **59** < **150**.

## Scope

P_GetMaxLineSpecial, P_FindLineSpecial binary search, P_ExecuteSpecial Retired (2026-10-23): out-of-body credit removed; honest **59** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f1998667230b4dfdd3c702a689eaa9af6ea77bcb`.

## Padding review (2026-10-21)



### Out-of-body uncredit (444)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **75** |
| **Out-of-body removed** | **16** |
| **Honest** | **59** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 444 --to 444` (0 hard / 0 soft).

## Seams

Live `LineSpecials[]` function pointers and sorted name table are not hosted.

## Credit rationale

Credits table bounds, info lookup, binary-search steps, TeleportSpecial alias, execute range gate, and VM ExecuteSpecial return wiring from `p_lnspec.cpp`.
