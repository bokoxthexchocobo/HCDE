# Chunk 381 — DAutomap DrawMarker texture flip, rotation, and framebuffer mapping

## Scope

DAutomap DrawMarker texture flip, rotation, and framebuffer mapping

## Native ranges

- `src/am_map.cpp` **DAutomap::DrawMarker** 3178–3207 (30 lines)

**Credited native LOC:** 30 (tally `nativeLoc` 30).

## Managed

(see conversion commit file list)
Managed LOC (tally): 61.
Conversion commit: `12eb8fee4fc8dc15a55a938767192a68c02063af`.

## Tests

- `VmAutomapDrawMarkerTests.NormalizeHorizontalFlip_FlipsWhenNegativeScale`
- `VmAutomapDrawMarkerTests.MapToFramebuffer_AppliesFrameOffset`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 381 --to 381` (0 hard / 0 soft).

## Seams

`DrawTexture` and `twod` clip tags are not hosted.

## Credit rationale

Credits `DAutomap::DrawMarker` from `am_map.cpp`.
