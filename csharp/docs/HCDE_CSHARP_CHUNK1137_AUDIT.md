# Chunk 1137 — Red poly-seg strokes, 3D-floor boundary, and teleport/exit probes from `am_map.cpp` (**2401–2550**); follows **1136**.

## Scope

Red poly-seg strokes, 3D-floor boundary, and teleport/exit probes from `am_map.cpp` (**2401–2550**); follows **1136**.

## Native ranges

- `src/am_map.cpp` **Automap floor boundary** 2401–2550 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmAmMapFloorBoundary1137Slice.cs`
Managed LOC (tally): 164.
Conversion commit: `bfbd6d94a0baeec825cde0a4955fa5b281c01618`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Red poly-seg loop (**2401–2405**) | yes |
| `AM_Check3DFloors` (**2418–2474**) | yes |
| `AM_checkSectorActions` (**2479–2496**) | yes |
| `AM_checkSpecialBoundary` (**2501–2531**) | yes |
| Teleport and exit predicates (**2533–2550**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2251–2400** | shade and debug gate already credited by **1136** |
| **2551–2553** | closing `Exit_Secret` token of the predicate ported above; not a separate credited line |
| **2555+** | trigger and lock boundary probes |

`FF_EXISTS` is **1** and `FF_THISINSIDE` is **0x40000000**. `SPAC_PlayerActivate` is **1243**. `SECSPAC_Use` is **16**, `SECSPAC_UseWall` is **32**, and `MF_FRIENDLY` is **0x08000000**. A one-sided line requires a use-activated sector action. Teleport specials are **70**, **71**, **39**, and **215**. Exit specials are **74**, **75**, **243**, and **244** (the last token sits on **2551** and is part of this predicate).

| Managed (`VmAmMapFloorBoundary1137Slice.cs`) | Native (`am_map.cpp`) |
|--------------------------------------------------|---------------------|
| Boundaries and special probes | **2401–2550** |
| Band hull (exclusive slice) | **2401–2550** |

### Chunk **1137** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1136** |


**Post-review ratio:** 164/150 managed/native ≈ 109% (src file LOC at HEAD; see tally).

## Tests

- `VmAmMapFloorBoundary1137SliceTests.RedPolyStrokes_DrawOnlyTheShownSubsector`
- `VmAmMapFloorBoundary1137SliceTests.HasFloorBoundary_MatchesModelAndFlags`
- `VmAmMapFloorBoundary1137SliceTests.SectorActions_RequireUseOnOneSidedLinesAndSkipFriends`
- `VmAmMapFloorBoundary1137SliceTests.TeleportAndExitBoundaries_PreferTheLineThenTheBackSector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1137 --to 1137` (0 hard / 0 soft).

## Seams

Model identity and the special predicate are host-supplied. Trigger-line filtering begins at **2555**.

## Credit rationale

UNCredited **`am_map.cpp`** **2401–2550** (+150), inside the 3594-line file, with no conversion or recognition overlap.
