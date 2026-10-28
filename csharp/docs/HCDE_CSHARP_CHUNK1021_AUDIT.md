# Chunk 1021 — `_Line` / `_Sector` GetHealth and SetHealth ZScript actions from `p_destructible.cpp` (**918–1067**, hull ends at EOF). **Zero credit:** `FDestructible` DEFINE cluster (**918–982**) on `VmPDestructibleSerializeScript` (retired **691**).

## Scope

`_Line` / `_Sector` GetHealth and SetHealth ZScript actions from `p_destructible.cpp` (**918–1067**, hull ends at EOF). **Zero credit:** `FDestructible` DEFINE cluster (**918–982**) on `VmPDestructibleSerializeScript` (retired **691**).

## Native ranges

- `src/playsim/p_destructible.cpp` **_Line / _Sector GetHealth SetHealth** 918–1067 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPDestructibleLineSectorHealth1021Slice.cs`
Managed LOC (tally): 119.
Conversion commit: `eab97a9ca1a79f67c4ccb8937e6f383ef91cd2dc`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `_Line::GetHealth` group vs line (**987–993**) | yes |
| `_Line::SetHealth` clamp + group propagate (**996–1011**) | yes |
| `_Sector::GetHealth` floor/ceiling/3D switch (**1014–1031**) | yes |
| `_Sector::SetHealth` part routing + group propagate (**1034–1066**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **918–982** | `FDestructible` / check DEFINE rows — **`VmPDestructibleSerializeScript`** + **`VmPDestructibleVulnerability`** (retired **690**/**691**) |
| **823–917** | serialize / `FHealthGroup` field exports outside hull |

| Managed (`VmPDestructibleLineSectorHealth1021Slice.cs`) | Native (`p_destructible.cpp`) |
|--------------------------------------------------------|-------------------------------|
| `ResolveLineGetHealth` | **987–991** |
| `ApplyLineSetHealth` | **1001–1008** |
| `ResolveSectorGetHealth` | **1021–1030** |
| `RouteSectorSetHealth` | **1040–1065** |
| Band hull (exclusive slice) | **918–1067** |

### Chunk **1021** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1020** |


**Post-review ratio:** 119/150 managed/native ≈ 79% (src file LOC at HEAD; see tally).

## Tests

- `VmPDestructibleLineSectorHealth1021SliceTests.ResolveLineGetHealth_PrefersGroupWhenLinked`
- `VmPDestructibleLineSectorHealth1021SliceTests.ApplyLineSetHealth_ClampsAndFlagsGroup`
- `VmPDestructibleLineSectorHealth1021SliceTests.ResolveSectorGetHealth_MatchesNativeSwitch`
- `VmPDestructibleLineSectorHealth1021SliceTests.RouteSectorSetHealth_SelectsGroupPerPart`
- `VmPDestructibleLineSectorHealth1021SliceTests.ShouldPropagateGroupHealth_WhenGroupNonZero`
- `VmPDestructibleLineSectorHealth1021SliceTests.BuildLineSetHealthMutation_MatchesNativeGroupPath`
- `VmPDestructibleLineSectorHealth1021SliceTests.BuildSectorSetHealthMutation_InvalidPartNotHandled`
- `VmPDestructibleLineSectorHealth1021SliceTests.ExecuteSectorGetHealth_UsesLookupPerPart`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1021 --to 1021` (0 hard / 0 soft).

## Seams

`PARAM_*`, `ACTION_RETURN_*`, and `P_SetHealthGroupHealth` host calls remain native.

## Credit rationale

Tally-UNCredited destructible EOF tail; exclusive slice for line/sector health getters/setters deferred in chunk **690** audit seams.
