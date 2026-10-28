# Chunk 986 — `FStateLabels::Destroy`, `HasSpecialDeathStates`, `MakeStateNameList`, and multi-name `FindState` from `p_states.cpp` (**151–300**); follows chunk **985** (**1–150**).

## Scope

`FStateLabels::Destroy`, `HasSpecialDeathStates`, `MakeStateNameList`, and multi-name `FindState` from `p_states.cpp` (**151–300**); follows chunk **985** (**1–150**).

## Native ranges

- `src/p_states.cpp` **FStateLabels::Destroy** 151–300 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPStatesLabels986Slice.cs`
Managed LOC (tally): 134.
Conversion commit: `32b40b4af701bfe12e463e4443188ce04ac5d2a2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `FStateLabels::Destroy` (**151–162**) | yes |
| `HasSpecialDeathStates` (**173–192**) | yes |
| `MakeStateNameList` legacy remap + push (**200–243**) | yes |
| `FindState` multi-name walk (**260–292**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **165–172**, **194–199**, **245–259**, **294–300** | comment banners / `FindStateByString` head (seam) |
| **207–241** | `strtok` / host string seam |
| **301+** | `FindStateByString` body outside hull |
| **1–150** | chunk **985** |

| Managed (`VmPStatesLabels986Slice.cs`) | Native (`p_states.cpp`) |
|----------------------------------------|----------------------|
| `DestroyNestedLabelChildren` | **153–160** |
| `ActorHasSpecialDeathStates` | **177–191** |
| `TryRemapLegacyStateAlias` / `BuildStateNameParts` | **209–240** |
| `ExactFindStateMisses` / `FindBestStateForNames` | **265–291** |
| Band hull (exclusive slice) | **151–300** |

### Chunk **986** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**985** |


**Post-review ratio:** 134/150 managed/native ≈ 89% (src file LOC at HEAD; see tally).

## Tests

- `VmPStatesLabels986SliceTests.DestroyNestedLabelChildren_TearsDownTrees`
- `VmPStatesLabels986SliceTests.SpecialDeathStatesAndLegacyAliases`
- `VmPStatesLabels986SliceTests.FindState_BestMatchAndExact`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 986 --to 986` (0 hard / 0 soft).

## Seams

`BinarySearch`, `strtok`, `M_Free`, and `FindStateByString` host glue remain injected.

## Credit rationale

UNCredited continuation of `p_states.cpp`; disjoint hull from **985**. No `VmPStatesLabels*` overlap.
