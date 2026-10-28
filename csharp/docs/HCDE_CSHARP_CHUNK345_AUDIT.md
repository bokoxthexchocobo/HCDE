# Chunk 345 — WarpTrans secret visit Reset flags2 (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **33** < **150**.

## Scope

WarpTrans secret visit Reset flags2 (trimmed) Retired (2026-10-23): out-of-body credit removed; honest **33** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `183f60e772e248fd4d69553e285e721a941470ba`.

## Padding review (2026-10-21)



### Out-of-body uncredit (345)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **33** |
| **Out-of-body removed** | **0** |
| **Honest** | **33** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 345 --to 345` (0 hard / 0 soft).

## Seams

Snapshot `Clean` and ACS string locks are modeled as policy hooks; full `level_info_t` storage stays native-shaped drafts.

## Credit rationale

Credits `CheckWarpTransMap`, `SecretLevelVisited`, and Hexen/Doom `flags2` default in `level_info_t::Reset` only.
