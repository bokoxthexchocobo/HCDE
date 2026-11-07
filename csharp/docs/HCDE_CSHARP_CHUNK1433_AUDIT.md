# Chunk 1433 — putc through openwads in m_misc.cpp (440-802).

## Scope

putc through openwads in m_misc.cpp (440-802).

## Native ranges

- `src/m_misc.cpp` **putc** 440–802 (363 lines)

**Credited native LOC:** 363 (tally `nativeLoc` 363).

## Managed

`VmMisc1433Slice.cs`
Managed LOC (tally): 300.
Conversion commit: `558a357c1f9ff57e871bd0ef68c380566d157e34`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| PCX writer (**440–615**) | yes |
| Free names and screenshot (**617–740**) | yes |
| Folders and config action (**742–802**) | yes |
| Band hull (**440–802**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| PCX struct | lines 414-437 stay uncredited |
| G_ScreenShot | callee in g_game.cpp |
| PNG helpers and shell folder | callees |

A PCX file starts with a 128-byte header: manufacturer 10, version 5, encoding 1, 8 bits, origin 0, size minus one, 75 dpi, one plane for a palette image and three otherwise, and a padded stride. Palette and RGB rows are packed with runs. A run longer than 63 splits. A lone byte below 192 is raw. An odd width writes a zero pad. An unknown color type returns after the header. A palette image then writes marker 12 and 256 colors. A null palette crashes there. PNG text is HCDE plus the version, capped at 99 characters. Any failed PNG step prints the screenshot error, and later steps are skipped. A free name tries i from 0 through 9999. With no clock the index is four digits. With a clock the first name uses the timestamp and later names add a two-digit index. An explicit file gains .pcx or .png only when the file part has no dot. A missing buffer prints that the screenshot could not be created unless quiet. A failed open names the path. A captured line prints the suffix after the last colon or slash unless long names are on. The screenshot command passes null when it has one argument and otherwise the next argument. Opening screenshots or wads resolves the directory, adds a trailing slash, and opens that folder. Both config actions save with a null filename.

| Managed (`VmMisc1433Slice.cs`) | Native (`m_misc.cpp`) |
|---------------------------------|----------------------|
| Screenshot writer and folders | **440–802** |
| Band hull (exclusive slice) | **440–802** |

### Chunk **1433** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **363** |
| **Net headline** | **+363** vs the prior chunk |


**Post-review ratio:** 300/363 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `VmMisc1433SliceTests.PcxRowsAndPngText`
- `VmMisc1433SliceTests.ShotNamesFoldersAndConfig`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1433 --to 1433` (0 hard / 0 soft).

## Seams

The PCX struct above this band stays uncredited. G_ScreenShot, PNG helpers, and the shell folder are callees.

## Credit rationale

UNCredited m_misc.cpp 440-802 (+363). This is the screenshot writer, free-name search, and folder commands. The band is shorter than 400 lines and is not sliced.
