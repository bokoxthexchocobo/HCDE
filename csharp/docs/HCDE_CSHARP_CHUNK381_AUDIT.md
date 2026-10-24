# Chunk 381 — DAutomap DrawMarker texture flip, rotation, and framebuffer mapping Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Scope

DAutomap DrawMarker texture flip, rotation, and framebuffer mapping Retired (2026-10-23): out-of-body credit removed; honest **0** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `90bc78ee8b5ce8f001fa30447ac800a7562261bd`.

## Padding review (2026-10-21)



### Out-of-body uncredit (381)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **30** |
| **Out-of-body removed** | **30** |
| **Honest** | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 381 --to 381` (0 hard / 0 soft).

## Seams

`DrawTexture` and `twod` clip tags are not hosted.

## Credit rationale

Credits `DAutomap::DrawMarker` from `am_map.cpp`.
