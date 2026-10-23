# Chunk 851 — `DFraggleThinker::InitFunctions` registration tail from `fragglescript/t_func.cpp` (4017–4231).

## Scope

`DFraggleThinker::InitFunctions` registration tail from `fragglescript/t_func.cpp` (4017–4231).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **DFraggleThinker::InitFunctions** 4017–4231 (215 lines)

**Credited native LOC:** 215 (tally `nativeLoc` 215).

## Managed

`VmFScriptInitFunctions.cs`, `VmFScriptParserSfCamera.cs`, `VmFScriptParserSfCombat.cs`
Managed LOC (tally): 309.
Conversion commit: `c0f62214e51ca66d8e95d9408c9cb68ca4559f38`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `DFraggleThinker::InitFunctions` | yes (name→delegate registration) |

| Managed | Native (`t_func.cpp` 4017–4231) |
|---------|-------------------------|
| `RegisterFraggleFunctions` | **4017–4231** |

**Also implemented (not in this band’s nativeRanges):** `SfMoveCamera` (**2685–2777**), `SfResurrect` / `SfLineAttack` (**3259–3316**) — registered by InitFunctions but native credit deferred (sub-150 islands).

**Post-review ratio:** 309/215 managed/native ≈ 144% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptInitFunctionsTests.InitFunctions_ResolvesActorTypesAndLineSpecs`
- `VmFScriptInitFunctionsTests.RegisterFraggleFunctions_BindsMoveCamera`
- `VmFScriptParserSfCameraTests.SfMoveCamera_MovesTowardTarget`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 851 --to 851` (0 hard / 0 soft).

## Seams

`VmFScriptInitFunctions.Register` binds FraggleScript names to existing `VmFScriptParser.Sf*` ports (no duplicate bodies).

## Credit rationale

Honest registration band after **850** script waits; **2685–2777** `movecamera` and **3259–3316** combat remain uncredited islands until a ≥150 contiguous envelope opens.
