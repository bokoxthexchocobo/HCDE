# Chunk 436 — LineSpecials[] dispatch table indices 0–149 Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

LineSpecials[] dispatch table indices 0–149 Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `eb9e80474036fd35b418b24dd2ea37e144582ab0`.

## Padding review (2026-10-21)



### Out-of-body uncredit (436)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **150** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 436 --to 436` (0 hard / 0 soft).

## Seams

Live `lnSpecFunc` pointers are not hosted; index taxonomy only.

## Credit rationale

Credits NOP/polyobj/door/floor/ceiling/plat/teleport/ACS/thing/light classification for `LineSpecials[]` slots from `p_lnspec.cpp`.
