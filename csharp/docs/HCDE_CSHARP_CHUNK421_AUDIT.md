# Chunk 421 — Sector rotation, line align, texture offset/scale, line blocking flags Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

Sector rotation, line align, texture offset/scale, line blocking flags Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `b75f7681772112b700d1ecaf6f33aeb3d5e1b81e`.

## Padding review (2026-10-21)



### Out-of-body uncredit (421)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **194** |
| **Out-of-body removed** | **194** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 421 --to 421` (0 hard / 0 soft).

## Seams

`AlignFlat`, `side_t` mutation, and live line iterators are not hosted.

## Credit rationale

Credits angle assignment, align side selection, 65536 texture deltas, and `flagtrans` set/clear wiring from `p_lnspec.cpp`.
