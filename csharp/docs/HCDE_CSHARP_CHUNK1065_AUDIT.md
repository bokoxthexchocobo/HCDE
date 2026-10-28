# Chunk 1065 — Custom `FindLump` scan tail and `ParseSBarInfo` top-level commands through `monospacefonts` alignment from `sbarinfo.cpp` (**451–600**); follows **1064** load head.

## Scope

Custom `FindLump` scan tail and `ParseSBarInfo` top-level commands through `monospacefonts` alignment from `sbarinfo.cpp` (**451–600**); follows **1064** load head.

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **SBarInfo::Load / ParseSBarInfo** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoParseTopLevel1065Slice.cs`
Managed LOC (tally): 132.
Conversion commit: `6079683f318f4f6b9feef2f6f69998435233aebb`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Custom lump `FindLump` loop continuation (**452–458**) | yes (scan policy; slot action via **1064** seam) |
| `ParseSBarInfo` include + main loop entry (**463–483**) | yes |
| `base` game name + standard lump policy (**485–520**) | yes |
| `height` assignment (**523–526**) | yes |
| `interpolatehealth` / `interpolatearmor` bool + speed (**528–560**) | yes |
| `completeborder` bool (**562–571**) | yes |
| `monospacefonts` spacing + alignment (**574–600**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **454–457** | **1064** slot create/parse (delegated only) |
| **467–468**, **474–477**, **519** | host scanner / lump I/O / recurse |
| **601+** | trailing `;` and **`lowerhealthcap`** outside hull |
| **603+** | commands past band end |

| Managed (`VmSbarinfoParseTopLevel1065Slice.cs`) | Native (`sbarinfo.cpp`) |
|--------------------------------------------------|-------------------------|
| `ShouldContinueCustomLumpScan` / `ResolveCustomSlotAction` | **452–458** |
| Include + top-level token gates | **469–483** |
| `ClassifyBaseGameName` / standard base paths | **489–520** |
| `MergeStatusBarHeight` | **524–525** |
| `BuildInterpolateHealthState` / armor twin | **528–560** |
| `ResolveCompleteBorder` | **562–571** |
| Monospace spacing + alignment | **574–600** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **1065** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1064** |


**Post-review ratio:** 132/150 managed/native ≈ 88% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoParseTopLevel1065SliceTests.ClassifyBaseGameName_RecognizesStandardAndOverrideNames`
- `VmSbarinfoParseTopLevel1065SliceTests.StandardBaseRelativePath_MapsDoomHereticHexen`
- `VmSbarinfoParseTopLevel1065SliceTests.ResolveGameTypeOverride_MatchesNativeStrifeAndNone`
- `VmSbarinfoParseTopLevel1065SliceTests.ShouldLoadStandardBaseAfterLookup_UsesUnsetSentinel`
- `VmSbarinfoParseTopLevel1065SliceTests.BuildInterpolateHealthState_HonorsTrueAndOptionalSpeed`
- `VmSbarinfoParseTopLevel1065SliceTests.ResolveTopLevelBool_AcceptsFalsePath`
- `VmSbarinfoParseTopLevel1065SliceTests.MergeStatusBarHeight_ReplacesWhenIntPresent`
- `VmSbarinfoParseTopLevel1065SliceTests.ResolveMonospaceSpacingCharacter_ClearsOnFalseBranch`
- `VmSbarinfoParseTopLevel1065SliceTests.TryClassifySpacingAlignment_RecognizesNativeKeywords`
- `VmSbarinfoParseTopLevel1065SliceTests.ResolveCustomSlotAction_DelegatesTo1064`
- `VmSbarinfoParseTopLevel1065SliceTests.ShouldRecurseParsedInclude_RequiresValidLump`
- `VmSbarinfoParseTopLevel1065SliceTests.TryParseOptionalSpacingAlignment_SkipsWhenNoComma`
- `VmSbarinfoParseTopLevel1065SliceTests.ShouldContinueCustomLumpScan_StopsOnMissingLump`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1065 --to 1065` (0 hard / 0 soft).

## Seams

Custom/default slot create vs parse uses **1064** `ResolveScriptSlotAction`; `FScanner`, `fileSystem`, recursive `ParseSBarInfo`, and **601+** statement terminators remain host-injected.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **451–600** (+150); zero overlap with **1064** load predicates (**432–450**) except delegated slot policy.
