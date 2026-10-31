# Chunk 1268 — Solid-segment clip list, the opening check, and the crunch from r_clipsegment.cpp (1-192).

## Scope

Solid-segment clip list, the opening check, and the crunch from r_clipsegment.cpp (1-192).

## Native ranges

- `src/rendering/swrenderer/segments/r_clipsegment.cpp` **RenderClipSegment** 1–192 (192 lines)

**Credited native LOC:** 192 (tally `nativeLoc` 192).

## Managed

`VmClip1268Slice.cs`
Managed LOC (tally): 153.
Conversion commit: `0e5e129fba3abad9f969dcbbc79a6ba8624b606c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Clear (**43–50**) | yes |
| Check (**52–74**) | yes |
| IsVisible (**76–93**) | yes |
| Clip (**95–191**) | yes |
| Band hull (**1–192**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Wall visitor | host callback |

Clear writes the left sentinel through the left edge and the right sentinel from the right edge through the maximum short. A span that does not cross a pixel is hidden. A span contained in one post is hidden and is not drawn. A visitor that rejects an otherwise open span leaves the posts unchanged. A solid span that ends on the next post slides that post's first edge back instead of inserting a new one. A solid span that sticks past a post extends that post, and a span that bridges two posts copies one slot past the cursor, leaving a trailing zero post in the list. A pass wall is drawn and does not close the opening. A missing visitor faults when a draw is required.

| Managed (`VmClip1268Slice.cs`) | Native (`r_clipsegment.cpp`) |
|---------------------------------|-------------------------------|
| Sentinels, check, and crunch | **1–192** |
| Band hull (exclusive slice) | **1–192** |

### Chunk **1268** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **192** |
| **Net headline** | **+192** vs post-**1267** |


**Post-review ratio:** 153/192 managed/native ≈ 80% (src file LOC at HEAD; see tally).

## Tests

- `VmClip1268SliceTests.Openings_InsertExtendAndSlideTheRightSentinel`
- `VmClip1268SliceTests.Crunch_KeepsTheSlotPastTheCursorAndAPassWallDoesNotClose`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1268 --to 1268` (0 hard / 0 soft).

## Seams

The wall visitor is a host callback. The slice keeps the sentinel posts, the contained-span rejection, the solid insert, and the crunch copy.

## Credit rationale

UNCredited r_clipsegment.cpp 1-192 (+192). A full tally line scan shows no overlap. The file ends at line 192.
