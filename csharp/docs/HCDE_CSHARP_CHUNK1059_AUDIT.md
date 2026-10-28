# Chunk 1059 — Lump-map font tail and `V_InitFontColors` TEXTCOLO range head from `v_font.cpp` (**301–450**); follows chunk **1058** (**151–300**).

## Scope

Lump-map font tail and `V_InitFontColors` TEXTCOLO range head from `v_font.cpp` (**301–450**); follows chunk **1058** (**151–300**).

## Native ranges

- `src/common/fonts/v_font.cpp` **V_InitCustomFonts tail / V_InitFontColors** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmVFontFontDefsTextColo1059Slice.cs`
Managed LOC (tally): 110.
Conversion commit: `6a21259f435f6f7497440efc6fa095b6963ab9dc`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Lump-map first/count scan (**299–315**) | yes |
| Lump-map instantiate gate + spacewidth (**315–325**) | yes (predicates) |
| Invalid property combo error tail (**327–334**) | seam (host `ScriptError`) |
| TEXTCOLO name alias gate (**378–386**) | yes |
| Console range switch (**396–405**) | yes |
| RGB pair split (**415–425**) | yes |
| Explicit range validation (**427–453**) | yes |
| Implicit range fill (**455–462**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **301–298** | seam with **1058** scan head |
| **317–324** | `CreateSpecialFont` / `FFont` host |
| **329–331**, **367–369** | lump open / loop host |
| **408–411** | `V_GetColor` host |
| **467+** | parm length / untranslated tail outside hull |

| Managed (`VmVFontFontDefsTextColo1059Slice.cs`) | Native (`v_font.cpp`) |
|--------------------------------------------------|----------------------|
| `ComputeLumpMapGlyphSpan` | **299–314** |
| `ShouldInstantiateLumpMapFont` / `ShouldApplyExplicitSpaceWidth` | **315–321** |
| `RejectsAliasForUntranslated` | **381–384** |
| `TrySwitchToConsoleRange` | **398–401** |
| `SplitRgb` | **416–425** |
| `ValidateExplicitColorRange` | **430–453** |
| `TryBuildImplicitColorRange` | **457–462** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **1059** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1058** |


**Post-review ratio:** 110/150 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmVFontFontDefsTextColo1059SliceTests.ComputeLumpMapGlyphSpan_FindsFirstAndCount`
- `VmVFontFontDefsTextColo1059SliceTests.ComputeLumpMapGlyphSpan_EmptyReturnsZeroCount`
- `VmVFontFontDefsTextColo1059SliceTests.RejectsAliasForUntranslated_WhenExtraNamesPresent`
- `VmVFontFontDefsTextColo1059SliceTests.TrySwitchToConsoleRange_RejectsDuplicateConsoleBlock`
- `VmVFontFontDefsTextColo1059SliceTests.ValidateExplicitColorRange_EnforcesNativeOrdering`
- `VmVFontFontDefsTextColo1059SliceTests.TryBuildImplicitColorRange_MatchesNativeTail`
- `VmVFontFontDefsTextColo1059SliceTests.SplitRgb_MatchesNativeParts`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1059 --to 1059` (0 hard / 0 soft).

## Seams

`CreateSpecialFont`, `FFont` setters, `fileSystem.FindLump`, `FScanner`, and `V_GetColor` remain host-injected; **451+** continues TEXTCOLO block validation outside hull.

## Credit rationale

UNCredited contiguous **`v_font.cpp`** **301–450** (+150); zero conversion/recognition overlap; no Vm* collision; same TU continuation after **1058**.
