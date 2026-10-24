# Chunk 851 — `DFraggleThinker::InitFunctions` from `fragglescript/t_func.cpp` (4017–4231).

## Scope

`DFraggleThinker::InitFunctions` from `fragglescript/t_func.cpp` (4017–4231).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **DFraggleThinker::InitFunctions** 4017–4231 (215 lines)

**Credited native LOC:** 215 (tally `nativeLoc` 215).

## Managed

`VmFScriptInitFunctions.cs`
Managed LOC (tally): 276.
Conversion commit: `4f1f8fd212a6e8047dcb0fbf7b82d874e8d4243d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ActorNames_init` → `ActorTypes[]` loop | yes |
| `consoleplayer` / `displayplayer` / `zoom` / `fov` / `trigger` globals | yes (`FraggleInitBindings` fields) |
| Line-special `NewVariable` loop | yes (when host supplies descriptors) |
| `NewFunction` for already-ported `SF_*` | yes (name → `Sf*` delegate) |
| `NewFunction` for not-yet-ported `SF_*` (e.g. `movecamera`, sector/door/HU hosts) | no — deferred to future chunks |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
| `ActorNamesInit` | **60–201** (data used at **4019–4022**) |
| `InitForThinker` actor resolve | **4019–4022** |
| `InitForThinker` global bindings | **4027–4031** |
| `InitForThinker` line-spec constants | **4034–4043** |
| `RegisterFraggleFunctions` / `BuildTable` | **4046–4231** (rows only for ported `Sf*`) |
| (header / braces) | **4011–4018**, **4230–4231** |

### Chunk **851** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` (gross band **4017–4231**) | **215** |
| `managedSourceFiles` (HEAD) | `VmFScriptInitFunctions.cs` only |
| **Net headline** | **+215** `convertedBehaviorNative` vs post-**850** tally (expected at land sync) |

**851 correction (pre-land):** Removed erroneous credit of deferred-band bodies (`VmFScriptParserSfCamera.cs` / `VmFScriptParserSfCombat.cs`); `ConstructThinker` now calls `InitForThinker` (no empty `InitFunctions` seam).

**Post-review ratio:** 276/215 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptInitFunctionsTests.ConstructThinker_RegistersFraggleFunctions`
- `VmFScriptInitFunctionsTests.ActorNamesInit_MatchesNativeTable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 851 --to 851` (0 hard / 0 soft).

## Seams

`VmFScriptThinker.ConstructThinker` (chunk **836** host) calls `VmFScriptInitFunctions.InitForThinker`; function bodies remain in prior `Sf*` chunks. **`movecamera`**, **`resurrect`**, **`lineattack`**, and other not-yet-ported `SF_*` are not registered until their native bands land.

## Credit rationale

Registration band after **850**; deferred islands (**2685–2777**, **3259–3316**, **1381–2241**, **3454–3472**) stay out of this chunk.
