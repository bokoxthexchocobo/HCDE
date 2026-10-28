# Chunk 1015 — `AirControlChanged`, `G_WriteSnapshots`, `G_WriteVisited`, `G_ReadSnapshots`, and `G_ReadVisited` head from `g_level.cpp` (**2109–2258**); follows chunk **1014** (**1959–2108**).

## Scope

`AirControlChanged`, `G_WriteSnapshots`, `G_WriteVisited`, `G_ReadSnapshots`, and `G_ReadVisited` head from `g_level.cpp` (**2109–2258**); follows chunk **1014** (**1959–2108**).

## Native ranges

- `src/g_level.cpp` **G_WriteSnapshots** 2109–2258 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGLevelSnapshotPersist1015Slice.cs`
Managed LOC (tally): 113.
Conversion commit: `037e7ce39917078378c882c8f0bbaa4ec160c8ef`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `AirControlChanged` (**2115–2125**) | yes |
| `G_WriteSnapshots` (**2133–2155**) | yes |
| `G_WriteVisited` (**2162–2195**) | yes |
| `G_ReadSnapshots` (**2202–2234**) | yes |
| `G_ReadVisited` head (**2241–2258**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2109–2114** | comment seam after **1014** `CalcMapName` |
| **2164–2174**, **2243–2253** | serializer host calls |
| **2207**, **2220**, **2230** | resource read host |
| **2259+** | `G_ReadVisited` playerclass body (**1016** follow-on) |

| Managed (`VmGLevelSnapshotPersist1015Slice.cs`) | Native (`g_level.cpp`) |
|------------------------------------------------|----------------------|
| Air friction linear branch | **2117–2124** |
| Snapshot filename rules | **2140–2153**, **2212–2230** |
| Visited / random class gates | **2169–2180** |
| Player class key/index gates | **2185–2191** |
| Band hull (exclusive slice) | **2109–2258** |

### Chunk **1015** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1014** |


**Post-review ratio:** 113/150 managed/native ≈ 75% (src file LOC at HEAD; see tally).

## Tests

- `VmGLevelSnapshotPersist1015SliceTests.AirControlFriction`
- `VmGLevelSnapshotPersist1015SliceTests.SnapshotWritePaths`
- `VmGLevelSnapshotPersist1015SliceTests.VisitedAndPlayerClassPersistence`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1015 --to 1015` (0 hard / 0 soft).

## Seams

`TArray` push, `FSerializer`/`FResourceFile`, `FindLevelInfo`, `G_ClearSnapshots`, and `UpdatePlayerClass` remain injected.

## Credit rationale

UNCredited **2109–2258**; disjoint from chunk **8** headless-flow and **1014** Init/`CalcMapName`.
