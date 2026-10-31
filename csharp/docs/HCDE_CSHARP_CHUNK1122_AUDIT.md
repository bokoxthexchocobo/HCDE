# Chunk 1122 — Automap display CVARs, empty-space margin clamp, follow/grid/mark commands, and default colors from `am_map.cpp` (**151–300**); follows **1121**.

## Scope

Automap display CVARs, empty-space margin clamp, follow/grid/mark commands, and default colors from `am_map.cpp` (**151–300**); follows **1121**.

## Native ranges

- `src/am_map.cpp` **Automap display commands and colors** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapDisplay1122Slice.cs`
Managed LOC (tally): 131.
Conversion commit: `f200ebb239f4073b3b109da4ff91142553420730`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Display CVAR defaults (**153–166**, **191–198**) | yes |
| Empty-space margin clamp (**170–177**) | yes |
| Follow/grid/texture toggles (**201–217**) | yes |
| Set/clear marks, gobig, togglemap, zoom (**221–259**) | yes |
| Map and overlay colors through other-walls (**269–300**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **179–181**, **204**, **227**, **243** | host automap/printf |
| **301+** | remaining overlay colors outside hull |

| Managed (`VmAmMapDisplay1122Slice.cs`) | Native (`am_map.cpp`) |
|----------------------------------------|---------------------|
| Display defaults, commands, colors | **151–300** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1122** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1121** |


**Post-review ratio:** 131/150 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapDisplay1122SliceTests.ClampEmptySpaceMargin_LimitsToZeroThroughNinety`
- `VmAmMapDisplay1122SliceTests.ToggleFollow_FlipsAndTryAddMark_RejectsNegative`
- `VmAmMapDisplay1122SliceTests.ShouldToggleMap_OnlyWhenGameActionIsIdle`
- `VmAmMapDisplay1122SliceTests.TrySetZoomDirection_ParsesSecondArgument`
- `VmAmMapDisplay1122SliceTests.DefaultColors_IncludeWallAndOverlayWall`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1122 --to 1122` (0 hard / 0 soft).

## Seams

`StatusBar`, `Printf`, and `AM_ToggleMap` remain host-injected; overlay colors continue at **301**.

## Credit rationale

UNCredited **`am_map.cpp`** **151–300** (+150), inside the 3594-line file, with no conversion or recognition overlap.
