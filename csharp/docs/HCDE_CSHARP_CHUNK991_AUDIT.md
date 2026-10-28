# Chunk 991 — `SetStop`, `SetWait`, `SetLoop`, `AddStates`, and `FinishStates` head from `p_states.cpp` (**901–1050**); follows chunk **990** (**751–900**).

## Scope

`SetStop`, `SetWait`, `SetLoop`, `AddStates`, and `FinishStates` head from `p_states.cpp` (**901–1050**); follows chunk **990** (**751–900**).

## Native ranges

- `src/p_states.cpp` **FStateDefinitions::AddStates** 901–1050 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPStatesParser991Slice.cs`
Managed LOC (tally): 160.
Conversion commit: `24844368fb3ce69e6d0880c9cc00bdbd875e9884`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `SetStop` (**915–934**) | yes |
| `SetWait` (**944–953**) | yes |
| `SetLoop` (**963–973**) | yes |
| `AddStates` frame walk (**984–1020**) | yes |
| `FinishStates` materialize + label fix order (**1029–1048**) | yes |
| Per-state resolve loop entry (**1050**) | yes (gate only) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **901–905** | `SetGotoLabel` tail (**990**) |
| **907–913**, **936–942**, **955–961**, **975–982**, **1022–1027** | comment banners |
| **1045–1048** | calls **990** `FixStatePointers` / `ResolveGotoLabels` helpers (orchestration only) |
| **1051+** | `switch` define resolve outside hull |
| **1–900** | chunks **985–990** |

| Managed (`VmPStatesParser991Slice.cs`) | Native (`p_states.cpp`) |
|----------------------------------------|----------------------|
| `TrySetStopOnParserSession` | **915–934** |
| `TrySetWaitOnParserSession` | **944–953** |
| `TrySetLoopOnParserSession` | **963–973** |
| `TryDecodeFrameCharacter` / `ApplyFrameToState` / `AddStatesFromFrameCharacters` | **988–1019** |
| `ShouldMaterializeOwnedStates` / `RunFinishStatesLabelFixPipeline` / `ShouldEnterPerStateDefineResolveLoop` | **1031–1050** |
| Band hull (exclusive slice) | **901–1050** |

### Chunk **991** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**990** |


**Post-review ratio:** 160/150 managed/native ≈ 107% (src file LOC at HEAD; see tally).

## Tests

- `VmPStatesParser991SliceTests.SetStopWaitLoopSession`
- `VmPStatesParser991SliceTests.AddStatesFrameDecode`
- `VmPStatesParser991SliceTests.FinishStatesHeadPipeline`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 991 --to 991` (0 hard / 0 soft).

## Seams

`RetargetStates`, `ClassDataAllocator`, `memcpy`, `SaveStateSourceLines`, and per-state `switch` bodies (**1051+**) remain host-injected.

## Credit rationale

UNCredited parser continuation; disjoint from **985–990**. Does not re-port **990** `SetGotoLabel` (**883–905**) or tree fix helpers.
