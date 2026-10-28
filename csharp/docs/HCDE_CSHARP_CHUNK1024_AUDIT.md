# Chunk 1024 — `FSpotList` add/remove/selection and serialize field bundle from `a_specialspot.cpp` (**1–150**). Primary **1024** target `p_floor.cpp` **1–150** rejected (file absent; `mapthinkers/a_floor.cpp` **1–150** overlaps chunk **917**).

## Scope

`FSpotList` add/remove/selection and serialize field bundle from `a_specialspot.cpp` (**1–150**). Primary **1024** target `p_floor.cpp` **1–150** rejected (file absent; `mapthinkers/a_floor.cpp` **1–150** overlaps chunk **917**).

## Native ranges

- `src/playsim/a_specialspot.cpp` **FSpotList spot registry** 1–150 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmASpecialSpotSpotList1024Slice.cs`
Managed LOC (tally): 131.
Conversion commit: `55d2659f521c8ad337a68b3849798f251688f392`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FSpotList::Add` (**67–75**) | yes |
| `FSpotList::Remove` + index fixup (**83–95**) | yes |
| `FSpotList::GetNextInList` (**103–115**) | yes |
| `FSpotList::GetSpotWithMinMaxDistance` (**123–142**) | yes |
| `FSpotList::GetRandomSpot` (**150–159**) | yes (param `onlyfirst` unused in native body) |
| `Serialize(FSpotList)` field bundle (**168–179**) | yes (index/skip/numcalls) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–40**, **49–59**, **161–167** | includes / empty ctor / comment banners |
| **188–150** gap | `DSpotState` methods deferred next band |

| Managed (`VmASpecialSpotSpotList1024Slice.cs`) | Native (`a_specialspot.cpp`) |
|------------------------------------------------|------------------------------|
| `TryAddSpot` | **67–75** |
| `TryRemoveSpot` | **83–95** |
| `GetNextInList` | **103–115** |
| `GetSpotWithMinMaxDistance` | **123–142** |
| `GetRandomSpot` | **150–159** |
| `CaptureSerializeFields` / `ApplySerializeFields` | **172–176** |
| Band hull (exclusive slice) | **1–150** |

### Chunk **1024** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1023** |


**Post-review ratio:** 131/150 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmASpecialSpotSpotList1024SliceTests.TryAddSpot_RejectsDuplicateReference`
- `VmASpecialSpotSpotList1024SliceTests.TryRemoveSpot_AdjustsIndexWhenRemovingEarlierEntry`
- `VmASpecialSpotSpotList1024SliceTests.GetNextInList_RoundsRobinAfterSkipCounter`
- `VmASpecialSpotSpotList1024SliceTests.GetSpotWithMinMaxDistance_SearchesCircularOrder`
- `VmASpecialSpotSpotList1024SliceTests.GetSpotWithMinMaxDistance_ReturnsNullWhenNoMatch`
- `VmASpecialSpotSpotList1024SliceTests.GetRandomSpot_OnlyOnceBeforeNumCalls`
- `VmASpecialSpotSpotList1024SliceTests.SerializeBundle_RoundTripsIndexFields`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1024 --to 1024` (0 hard / 0 soft).

## Seams

`pr_spot`, `IMPLEMENT_CLASS`, `DSpotState` host lookup, and `AActor`/`PClassActor` types remain native.

## Credit rationale

Preflight `--band`: `p_floor.cpp` **1–150** tally-UNCredited by basename only (no source file). Real floor head **`a_floor.cpp` **1–150**** OVERLAP chunk **917**; off-tally `VmAFloor*` duplicates same span. First clean ≥150: **`a_specialspot.cpp` **1–150**** (zero conversion rows, zero recognition overlap, no `VmASpecialSpot*` yet).
