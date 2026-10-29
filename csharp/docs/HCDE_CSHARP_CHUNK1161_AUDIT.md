# Chunk 1161 — Multi-block thing iterator and line intercepts from `p_maputl.cpp` (**1027–1193**).

## Scope

Multi-block thing iterator and line intercepts from `p_maputl.cpp` (**1027–1193**).

## Native ranges

- `src/playsim/p_maputl.cpp` **Multi-block things and line intercepts** 1027–1193 (167 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmPMaputlIterator1161Slice.cs`
Managed LOC (tally): 270.
Conversion commit: `def53ea99df48aaad1f32ebc38f0e7b5ae158799`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Actor constructor (**1038–1046**) | yes |
| Coordinate constructor (**1048–1059**) | yes |
| `Next` (**1067–1102**) | yes |
| `startIteratorForGroup` (**1110–1117**) | yes |
| `Reset` (**1125–1131**) | yes |
| Static intercept storage (**1139**) | yes |
| `AddLineIntercepts` (**1154–1185**) | yes |
| Band hull (**1027–1193**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **949–1026** | `FBlockThingsIterator::Next` is already credited |
| **1194–1361** | `AddThingIntercepts` is already credited |
| `CollectConnectedGroups` | the portal walk stays host-supplied |
| `ignorerestricted` | the parameter is unused by both constructors |

`LOWER` is **0x4000**, `UPPER` is **0x8000**, and `FLAT` is **0xC000**. `FFCF_NOFLOOR` is **32**. `FFCF_NOCEILING` is **64**. `EQUAL_EPSILON` is **1/65536**.

An actor iterator stores the origin, and collects only when the checklist is not yet inited. The collect call sees the origin and the actor top, plus the radius argument, before `Z` is replaced. A check radius of **-1** stores the actor radius. Any other check radius is stored as `Z`. The base group is the origin sector. A coordinate iterator stores the supplied point. A missing sector is `PointInSector`. Collect, when needed, uses `z + height` as the upper bound, and `Z` is then replaced with the check radius even when that radius is **-1**. Both constructors finish in `Reset`.

`Reset` sets the index to **-1** and the portal flags to **0**, opens the base group, then clears the hash. Opening a group adds the displacement of that group to the checkpoint, and the box radius is the checkpoint `Z`. The block iterator is initialized with hash clearing off. The same-group displacement is **(0, 0)** and does not ask the host. `Next` returns the current block actor with `XY` shifted by the actor group's displacement and `Z` left at the checkpoint. An exhausted block that is on the last checklist entry returns false and leaves the index. Otherwise the next entry's flat bits select the flags, the index advances, and the group id is the entry with those bits removed. Upper sets no-floor, lower sets no-ceiling, and any other pattern, including both bits, sets **0**. The call then continues. The native static intercept list is reserved to **128** entries and kept per traverse. A line whose endpoints are on the same side of the trace is skipped. The fraction runs along the trace. A zero denominator yields **0**. A fraction before the start or past **1** is skipped. A fraction equal to the start or equal to **1** is kept, marked as a line, and not done.

| Managed (`VmPMaputlIterator1161Slice.cs`) | Native (`p_maputl.cpp`) |
|-------------------------------------------|--------------------------|
| Iterator and line intercepts | **1027–1193** |
| Band hull (exclusive slice) | **1027–1193** |

### Chunk **1161** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **167** |
| **Net headline** | **+167** vs post-**1160** |


**Post-review ratio:** 270/167 managed/native ≈ 162% (src file LOC at HEAD; see tally).

## Tests

- `VmPMaputlIterator1161SliceTests.Actor_RewritesRadiusAndSkipsCollectWhenInited`
- `VmPMaputlIterator1161SliceTests.Point_ReadsTheSectorAndStoresTheGivenRadius`
- `VmPMaputlIterator1161SliceTests.Next_ReturnsTheBaseThingThenUpperAndLowerGroups`
- `VmPMaputlIterator1161SliceTests.Lines_KeepCrossingsInsideTheSpan`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1161 --to 1161` (0 hard / 0 soft).

## Seams

Connected-group collection, portal displacement, the block thing queue, and the block line iterator stay host-supplied. This slice builds the checkpoint, steps groups, and accepts a crossing.

## Credit rationale

UNCredited **`p_maputl.cpp`** **1027–1193** (+167), the only remaining hole of at least 150 lines in the file, with no conversion or recognition overlap.
