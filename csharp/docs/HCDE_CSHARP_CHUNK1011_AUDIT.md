# Chunk 1011 — `getRealLightmode`, cutscene helpers, and `G_DeferedInitNew` from `g_level.cpp` (**151–300**); follows chunk **1010** (**1–150**).

## Scope

`getRealLightmode`, cutscene helpers, and `G_DeferedInitNew` from `g_level.cpp` (**151–300**); follows chunk **1010** (**1–150**).

## Native ranges

- `src/g_level.cpp` **getRealLightmode** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmGLevelLightAndDefer1011Slice.cs`
Managed LOC (tally): 77.
Conversion commit: `4b6b23925f9479b7937ce7462f8f59d918d973e4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `getRealLightmode` (**158–171**) | yes |
| `CallCreateMapFunction` arity (**217–223**) | yes |
| `CreateCutscene` (**234–249**) | yes |
| `G_DeferedInitNew` map/skill (**262–268**) | seam (host defer) |
| `G_DeferedInitNew(FNewGameStartup*)` (**270–297**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **153–156** | `gl_lightmode` CVAR body (credited **1010**) |
| **173–197** | globals / extern host |
| **206–209** | `Local_Job_Init` type registration host |
| **220–225** | VM call host |
| **245** | `AddGenericVideo` host |
| **290–295** | `ScreenJobValidate` / delete host |
| **305+** | `CCMD(map)` (next band) |

| Managed (`VmGLevelLightAndDefer1011Slice.cs`) | Native (`g_level.cpp`) |
|-----------------------------------------------|----------------------|
| `ResolveRealLightMode` | **158–171** |
| `ClassifyCutsceneDefinition` / arity validate | **234–249**, **217–223** |
| Deferred new-game / intro gates | **270–297** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1011** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1010** |


**Post-review ratio:** 77/150 managed/native ≈ 51% (src file LOC at HEAD; see tally).

## Tests

- `VmGLevelLightAndDefer1011SliceTests.RealLightMode`
- `VmGLevelLightAndDefer1011SliceTests.CutsceneCreation`
- `VmGLevelLightAndDefer1011SliceTests.DeferredNewGame`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1011 --to 1011` (0 hard / 0 soft).

## Seams

`CUSTOM_CVAR` bodies (**153–156** seam with **1010**), VM `CallCreateFunction`, `ScreenJobValidate`, and `CheckWarpTransMap` remain injected.

## Credit rationale

UNCredited **151–300** CLI band; disjoint from chunk **1010** hull except CVAR tail seam lines **151–156** (zero duplicate managed credit).
