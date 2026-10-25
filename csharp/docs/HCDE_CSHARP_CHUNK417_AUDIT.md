# Chunk 417 — LS_Thing_Hate targeting policy Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

LS_Thing_Hate targeting policy Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `64b9b85ef2df5565c83d78a990cbc9dbf42e02a1`.

## Padding review (2026-10-21)



### Out-of-body uncredit (417)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **162** |
| **Out-of-body removed** | **162** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 417 --to 417` (0 hard / 0 soft).

## Seams

Actor iterators, `SeeState`, and live mobj flags are not hosted.

## Credit rationale

Credits `LS_Thing_Hate` hatee selection and `MF3`/`MF4` attitude flags from `p_lnspec.cpp`.
