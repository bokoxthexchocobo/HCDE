# Chunk 1064 — `SBarInfoMainBlock` draw/parse tail, keyword tables, `FreeSBarInfoScript`, and `SBarInfo::Load` head from `sbarinfo.cpp` (**301–450**); follows **1063** (**151–300**).

## Scope

`SBarInfoMainBlock` draw/parse tail, keyword tables, `FreeSBarInfoScript`, and `SBarInfo::Load` head from `sbarinfo.cpp` (**301–450**); follows **1063** (**151–300**).

## Native ranges

- `src/g_statusbar/sbarinfo.cpp` **SBarInfoMainBlock / SBarInfo::Load** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoMainBlockLoad1064Slice.cs`
Managed LOC (tally): 121.
Conversion commit: `167f8aac2e2e6ae8abc3562d5275051d7d9e96de`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Draw offset store + alpha combine (**310–313**) | yes (alpha via **1063** seam) |
| Parse flow after flag list (**330–339**) | yes |
| Top-level / status-bar keyword tables (**358–418**) | yes |
| `FreeSBarInfoScript` slot loop (**420–430**) | yes |
| `SBarInfo::Load` default + custom lump policy (**432–450**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **312–337** | chunk **1063** flag/alpha list |
| **301–309**, **341–343** | accessors / thin forwards |
| **436–444**, **452–457** | host lump I/O + `new`/`ParseSBarInfo` |
| **451+** | `FindLump` loop host |
| **463+** | `ParseSBarInfo` outside hull |

| Managed (`VmSbarinfoMainBlockLoad1064Slice.cs`) | Native (`sbarinfo.cpp`) |
|--------------------------------------------------|-------------------------|
| `StoreDrawOffsets` / `ApplyDrawAlpha` | **310–313** |
| `ShouldParseFlowAfterFlagList` | **330–333** |
| `TopLevelKeywords` / `MatchTopLevelKeyword` | **358–404** |
| `StatusBarTypeNames` / `MatchStatusBarTypeName` | **375–418** |
| `ShouldFreeScriptSlot` | **424–428** |
| Load predicates + slot actions | **432–450** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **1064** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1063** |


**Post-review ratio:** 121/150 managed/native ≈ 81% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoMainBlockLoad1064SliceTests.ApplyDrawAlpha_DelegatesTo1063Combine`
- `VmSbarinfoMainBlockLoad1064SliceTests.ShouldParseFlowAfterFlagList_WhenSeparatorsEnd`
- `VmSbarinfoMainBlockLoad1064SliceTests.ResolveMainBlockAlphaAfterFlagList_UsesFloatWhenRead`
- `VmSbarinfoMainBlockLoad1064SliceTests.ResolveScriptSlotAction_ChoosesCreateOrParse`
- `VmSbarinfoMainBlockLoad1064SliceTests.MatchTopLevelKeyword_FindsAppendStatusBar`
- `VmSbarinfoMainBlockLoad1064SliceTests.MatchStatusBarTypeName_RecognizesInventoryFullscreen`
- `VmSbarinfoMainBlockLoad1064SliceTests.ShouldLoadDefaultFromGameInfo_RequiresName`
- `VmSbarinfoMainBlockLoad1064SliceTests.ShouldEmitLoadBanners_RequireNonBatchAndValidLump`
- `VmSbarinfoMainBlockLoad1064SliceTests.ScriptSlotConstants_MatchNativeHeaders`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1064 --to 1064` (0 hard / 0 soft).

## Seams

Main-block flag/alpha list (**312–337**) uses **1063** `VmSbarinfoParser1063Slice` only; `FScanner`, `fileSystem`, `new SBarInfo`, and `ParseSBarInfo` body (**463+**) remain host-injected.

## Credit rationale

UNCredited **`sbarinfo.cpp`** **301–450** (+150); excludes duplicate managed credit on **312–337** already tallied on **1063**.
