# Chunk 364 — Automap framebuffer clear policy, Cohen-Sutherland clipMline, drawMline thickness/AA plan Retired (2026-10-24): thin managed at HEAD — surviving **0** < **114** (50% of honest **228**).

## Scope

Automap framebuffer clear policy, Cohen-Sutherland clipMline, drawMline thickness/AA plan Retired (2026-10-24): thin managed at HEAD — surviving **0** < **114** (50% of honest **228**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `8165c94934b04e33c1da32eb9a7f0533d6d1fc40`.

## Padding review (2026-10-21)



### Out-of-body uncredit (364)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **228** |
| **Out-of-body removed** | **0** |
| **Honest** | **228** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 364 --to 364` (0 hard / 0 soft).

## Seams

`twod` draw calls, `TexMan`, and live `DAutomap` member state are not hosted; managed code mirrors backdrop gating, map-to-frame clipping, and line draw parameters.

## Credit rationale

Credits `DAutomap::clearFB`, `clipMline`, and the clipped `drawMline` thickness/antialiasing policy from `am_map.cpp`.
