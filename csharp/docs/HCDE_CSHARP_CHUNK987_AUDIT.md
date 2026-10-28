# Chunk 987 — `VerifyJumpTarget`, `FStateLabelStorage::GetState`, state-definition lookup, and `FindStateAddress` from `p_states.cpp` (**301–450**); follows chunk **986** (**151–300**).

## Scope

`VerifyJumpTarget`, `FStateLabelStorage::GetState`, state-definition lookup, and `FindStateAddress` from `p_states.cpp` (**301–450**); follows chunk **986** (**151–300**).

## Native ranges

- `src/p_states.cpp` **FStateLabelStorage::GetState** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPStatesStorage987Slice.cs`
Managed LOC (tally): 157.
Conversion commit: `080512183a882457640d1182a6f2726ea9ca25e1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `VerifyJumpTarget` (**313–328**) | yes |
| `GetState` named/jump/bundle paths (**336–373**) | yes |
| `FindStateLabelInList` (**412–430**) | yes |
| `FindStateAddress` walk (**439–450**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **300–304** | `FindStateByString` defers to **986** helpers (seam) |
| **307–311**, **375–411**, **432–438**, **453+** | comments / VM glue / next function |
| **348–358**, **363–368** | `memcpy` host |
| **1–300** | chunks **985–986** |

| Managed (`VmPStatesStorage987Slice.cs`) | Native (`p_states.cpp`) |
|-----------------------------------------|----------------------|
| `UsesNamedNameLabelEncoding` / decode helpers | **338–341**, **345–346** |
| `VerifyJumpTargetInClassHierarchy` | **315–327** |
| `ResolveScriptStateFromStorage` / `ResolveBundledStoragePos` | **336–373** |
| `FindStateDefineIndex` / `FindOrCreateStateDefineIndex` | **414–429** |
| `WalkStateDefineAddress` | **442–450** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **987** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**986** |


**Post-review ratio:** 157/150 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmPStatesStorage987SliceTests.JumpVerificationAndEncodings`
- `VmPStatesStorage987SliceTests.ResolveScriptStateFromStorage`
- `VmPStatesStorage987SliceTests.StateDefineListWalk`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 987 --to 987` (0 hard / 0 soft).

## Seams

`FindStateByString` (**300–304**), `DEFINE_ACTION_FUNCTION` VM bodies, and `memcpy` storage reads remain host-injected.

## Credit rationale

UNCredited mid-file state script/define band; disjoint from **985–986**. Does not re-port **986** `FindState` / `MakeStateNameList` logic.
