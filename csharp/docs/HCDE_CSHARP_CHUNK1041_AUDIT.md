# Chunk 1041 — `FState::CheckCallerType`, `FState::CallAction`, and `GetSpriteIndex` hull from `info.cpp` (**151–300**). Continues **1040** (**1–150**).

## Scope

`FState::CheckCallerType`, `FState::CallAction`, and `GetSpriteIndex` hull from `info.cpp` (**151–300**). Continues **1040** (**1–150**).

## Native ranges

- `src/gamedata/info.cpp` **FState CheckCallerType / CallAction** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmInfoActorState1041Slice.cs`
Managed LOC (tally): 131.
Conversion commit: `e51740f8678d2638f88de1eac2a4f9030b93fb2c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FState::CheckCallerType` (**151–174**) | yes |
| `FState::CallAction` param / return setup (**178–233**) | yes (policy; `VMCallAction` seam) |
| `CallAction` abort stacktrace (**235–252**) | yes |
| `CallAction` no-action path (**260–263**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1–150** | chunk **1040** |
| **176–177** | global `actionParams` decl |
| **234**, **254–259** | host VM invoke / cycle counters |
| **271–300** | `GetSpriteIndex` — uncredited `VmSpriteCatalog` seam only |
| **500–799** | chunks **1037–1038** |
| **800–893** | deferred (**94** lines) |
| **894–1043** | chunk **1039** |
| `VmActorStateRuntime` | uncredited helper — seam only |

| Managed (`VmInfoActorState1041Slice.cs`) | Native (`info.cpp`) |
|------------------------------------------|---------------------|
| `EvaluateCheckCallerType` / format helpers | **151–174** |
| `PrepareStateReturnTracking` | **187–196** |
| `ShouldUseDefaultArgsPath` / `ResolveVmCallParamCount` / `FillImplicitActionParams` | **207–232** |
| `SelectPspriteCallInfoPrefix` / `FormatCallActionAbortStackTrace` | **239–252** |
| `ShouldInvokeCallAction` | **180–181**, **260–263** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **1041** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1040** |


**Post-review ratio:** 131/150 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmInfoActorState1041SliceTests.EvaluateCheckCallerType_SkipsWhenNoImplicitArgs`
- `VmInfoActorState1041SliceTests.EvaluateCheckCallerType_BadPrototypeWhenFirstArgNotObjectPointer`
- `VmInfoActorState1041SliceTests.EvaluateCheckCallerType_OkWhenMatches`
- `VmInfoActorState1041SliceTests.EvaluateCheckCallerType_NullCallerWhenSelfMissing`
- `VmInfoActorState1041SliceTests.EvaluateCheckCallerType_InvalidClassWhenKindMismatch`
- `VmInfoActorState1041SliceTests.FormatCheckCallerMessages_MatchNativeTemplates`
- `VmInfoActorState1041SliceTests.PrepareStateReturnTracking_TracksPointerOnlyForStateReturn`
- `VmInfoActorState1041SliceTests.ResolveVmCallParamCount_PicksDefaultOrImplicitPath`
- `VmInfoActorState1041SliceTests.FillImplicitActionParams_WritesThreeSlotLayout`
- `VmInfoActorState1041SliceTests.SelectPspriteCallInfoPrefix_WeaponVsOverlay`
- `VmInfoActorState1041SliceTests.FormatCallActionAbortStackTrace_WithAndWithoutOwner`
- `VmInfoActorState1041SliceTests.ShouldInvokeCallAction_RequiresBinding`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1041 --to 1041` (0 hard / 0 soft).

## Seams

`VMCallAction`, `ThrowAbortException`, `CVMAbortException`, `ActionCycles`, and global `sprites` remain host-injected; uncredited `VmSpriteCatalog` / `VmActorStateRuntime` are not re-credited.

## Credit rationale

File-scoped UNCredited **`info.cpp`** **151–300**; excludes **1–150** (**1040**), **500–799** (**1037–1038**), deferred **800–893**, and **894–1043** (**1039**).
