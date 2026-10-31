# Chunk 1188 — Doom font selection from doomfont.cpp (1–170).

## Scope

Doom font selection from doomfont.cpp (1–170).

## Native ranges

- `src/gamedata/doomfont.cpp` **InitDoomFonts** 1–170 (170 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmDoomFont1188Slice.cs`
Managed LOC (tally): 235.
Conversion commit: `e4216182cef1d8d32045c94a861003430b41aa56`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| font constants (**30–34**) | yes |
| InitDoomFonts (**36–170**) | yes |
| Band hull (**1–170**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| none | the band is the whole file |

A registered SmallFont is kept. Otherwise FONTA_S in the default namespace builds the small font, and a graphics FONTA33 from a file after the IWAD drops the file template. The same wad, or a missing FONTA33, keeps defsmallfont. Both of those set the cursor to '['. With no FONTA_S, STCFN033 in the graphics namespace builds the Doom-template font and does not set a cursor. OriginalSmallFont follows the same lumps and is always the IWAD copy. SmallFont2 uses STBFN033 only when it is not already registered. BigFont uses FONTB_S when it is not registered, then ZBIGFONT. OriginalBigFont uses FONTB_S or the generic bigfont. A folder BigUpper is deleted and replaced when BigFont is not a folder. A missing BigUpper aliases BigFont. A missing console font aliases the small font, and a still-missing console font is fatal before the later aliases. WINUM0 selects IntermissionFont_Doom; otherwise the intermission font aliases BigFont. Missing originals fall back to the console font, the small font, and the original big font.

| Managed (`VmDoomFont1188Slice.cs`) | Native (`doomfont.cpp`) |
|-------------------------------------|--------------------------|
| Font selection | **1–170** |
| Band hull (exclusive slice) | **1–170** |

### Chunk **1188** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **170** |
| **Net headline** | **+170** vs post-**1187** |


**Post-review ratio:** 235/170 managed/native ≈ 138% (src file LOC at HEAD; see tally).

## Tests

- `VmDoomFont1188SliceTests.Init_BuildsAPwadSmallFontAndKeepsTheOriginal`
- `VmDoomFont1188SliceTests.Init_UsesDoomTemplatesStrifeAndAFolderOverride`
- `VmDoomFont1188SliceTests.Init_FallsBackAndRejectsAMissingConsoleFont`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1188 --to 1188` (0 hard / 0 soft).

## Seams

Lump presence, wad container, V_GetFont, FindFont, and the WINUM0 patch are host results. The glyph count is 223, from '!' through byte 255.

## Credit rationale

UNCredited doomfont.cpp 1–170 (+170). The file ends at 170. A full tally line scan shows no overlap.
