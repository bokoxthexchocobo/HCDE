# Chunk 1309 — Build ART tile count, palette translation, and tile registration from buildloader.cpp (1-290).

## Scope

Build ART tile count, palette translation, and tile registration from buildloader.cpp (1-290).

## Native ranges

- `src/gamedata/textures/buildloader.cpp` **InitBuildTiles** 1–290 (290 lines)

**Credited native LOC:** 290 (tally `nativeLoc` 290).

## Managed

`VmBuildLoader1309Slice.cs`
Managed LOC (tally): 346.
Conversion commit: `10154f54877d39f5548c8fe5849631fae3910497`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| CountTiles (**48–60**) | yes |
| Build palette translation (**68–113**) | yes |
| AddTiles sizes, anim, and rotations (**124–219**) | yes |
| InitBuildTiles palette and ART scan (**230–289**) | yes |
| Band hull (**1–290**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| FBuildTexture pixel decode | texture callee; this band passes the pixel span, size, and offsets |
| BestColor | ColorMatcher.Pick receives the expanded channels |

CountTiles returns 0 unless the first little-endian word is 1 and the end tile is at least the start tile. A buffer shorter than the 16-byte header also returns 0. The palette lump shorter than 768 returns 0. Otherwise the first 765 bytes choose Blood when any is at least 64. VGA channels expand with (v<<2)|(v>>4). Blood channels shift left by 2. The stored palette keeps the low 8 bits, and Pick receives the untruncated integers. Entry 255 is transparent. The stored translation id is masked to 16 bits. AddTiles skips a nonpositive dimension without consuming pixels. The name is the path prefix plus BTIL and a four-digit tile number. Offsets add the signed anim byte to half the size. An anim runs when the low 6 bits and the 64/128/192 field are both set. Those fields select type 2, 0, or 1, and the speed is max(1, (1<<speed)*1000/120). Rotation 1 fills the 16-view frame and sets flip 0x00FC. Rotation 2 fills the 8-view frame and sets flip 0. Init skips a palette hidden by a later lump of the same full name. The base name is lowercased and must be palette.dat, and the file must be at least 768 bytes. A nonempty path that does not end in a slash gains one. ART files are tiles000.art through the first missing name, in the palette container only. A positive tile count is added to the total even when a zero-size tile was skipped inside AddTiles.

| Managed (`VmBuildLoader1309Slice.cs`) | Native (`buildloader.cpp`) |
|---------------------------------------|----------------------------|
| Count, palette, tiles, and the scan | **1–290** |
| Band hull (exclusive slice) | **1–290** |

### Chunk **1309** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **290** |
| **Net headline** | **+290** vs post-**1308** |


**Post-review ratio:** 346/290 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmBuildLoader1309SliceTests.Palette_ExpandsVgaAndShiftsBlood`
- `VmBuildLoader1309SliceTests.Init_StopsAtTheMissingArtAndCountsZeroTiles`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1309 --to 1309` (0 hard / 0 soft).

## Seams

ColorMatcher.Pick, StoreTranslation, and GetTranslation are palette callees. AddGameTexture and AddSimpleAnim are texture-manager callees. CheckNumForFullName searches the host lump list from the end, and the container overload also requires the palette container. Multi-byte art fields are little-endian, which is the in-memory word on this target.

## Credit rationale

UNCredited buildloader.cpp 1-290 (+290). A full tally line scan shows no overlap. The file ends at line 290.
