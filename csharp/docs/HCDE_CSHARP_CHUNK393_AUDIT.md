# Chunk 393 — dmflags3, compatflags refresh, HCDE rcon poll hook Retired (2026-10-23): out-of-body credit removed; honest **108** < **150**.

## Scope

dmflags3, compatflags refresh, HCDE rcon poll hook Retired (2026-10-23): out-of-body credit removed; honest **108** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `df9f72f61f001d94c57619f774982c072c26b010`.

## Padding review (2026-10-21)



### Out-of-body uncredit (393)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **116** |
| **Out-of-body removed** | **8** |
| **Honest** | **108** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 393 --to 393` (0 hard / 0 soft).

## Seams

`Level->ApplyCompatibility` and socket IO are not hosted.

## Credit rationale

Credits `CUSTOM_CVAR(dmflags3)`, `compatflags`/`compatflags2`, dmflags3 flag CVARs, and `HCDERconPollListener` from `d_main.cpp` / `d_net_rcon.cpp`.
