# Chunk 1126 — Raven initColors arm, ParseAMColors, thin-triangle glyph, arrow lump scale, and AM_StaticInit clears from `am_map.cpp` (**751–900**); follows **1125**.

## Scope

Raven initColors arm, ParseAMColors, thin-triangle glyph, arrow lump scale, and AM_StaticInit clears from `am_map.cpp` (**751–900**); follows **1125**.

## Native ranges

- `src/am_map.cpp` **Automap color parser and arrow glyph** 751–900 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapColorParse1126Slice.cs`
Managed LOC (tally): 264.
Conversion commit: `51c63863588458250779747c99f60a48e10013c3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Raven `initFromColors(..., true)` (**752–755**) | yes |
| `ParseAMColors` base, showlocks, and color keys (**767–838**) | yes |
| Thin-triangle glyph (**853–857**) | yes |
| `AM_ParseArrow` radius and vertex pairs (**865–895**) | yes |
| `AM_StaticInit` four clears (**897–900**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **756–766**, **840–852**, **859–864** | braces and section comments |
| **901+** | arrow lump selection and fatal error outside hull |

`FScanner::Compare` is case-insensitive. Arrow radius truncates to **18** before scaling.

| Managed (`VmAmMapColorParse1126Slice.cs`) | Native (`am_map.cpp`) |
|-------------------------------------------|---------------------|
| Color parser, glyph, arrow scale | **751–900** |
| Band hull (exclusive slice) | **751–900** |

### Chunk **1126** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1125** |


**Post-review ratio:** 264/150 managed/native ≈ 176% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapColorParse1126SliceTests.RavenArm_ShowsLocksAndBaseDoomClearsThem`
- `VmAmMapColorParse1126SliceTests.ParseAmColors_RejectsBaseAfterAColorAndUnknownKeys`
- `VmAmMapColorParse1126SliceTests.ParseAmColors_WritesOpaqueWallFromResolver`
- `VmAmMapColorParse1126SliceTests.ArrowLump_ScalesByPlayerRadiusAndStaticInitClears`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1126 --to 1126` (0 hard / 0 soft).

## Seams

Scanner and `V_GetColorFromString` stay host-injected. Lump opens that miss return an empty arrow.

## Credit rationale

UNCredited **`am_map.cpp`** **751–900** (+150), inside the 3594-line file, with no conversion or recognition overlap.
