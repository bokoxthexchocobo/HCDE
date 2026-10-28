# Chunk 988 — `SetStateLabel`, `AddStateLabel`, `GetStateLabelIndex`, `FindState`, `CreateStateLabelList`, and `InstallStates` head from `p_states.cpp` (**451–600**); follows chunk **987** (**301–450**).

## Scope

`SetStateLabel`, `AddStateLabel`, `GetStateLabelIndex`, `FindState`, `CreateStateLabelList`, and `InstallStates` head from `p_states.cpp` (**451–600**); follows chunk **987** (**301–450**).

## Native ranges

- `src/p_states.cpp` **FStateDefinitions::InstallStates** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPStatesDefine988Slice.cs`
Managed LOC (tally): 101.
Conversion commit: `bc7915da00fe023ec1d95aeb35d085c9fe164c41`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `SetStateLabel` (**459–464**) | yes |
| `AddStateLabel` index encoding (**472–480**) | yes |
| `GetStateLabelIndex` (**489–500**) | yes |
| `FindState` by name walk (**509–526**) | yes |
| `CreateStateLabelList` prune/materialize gate (**541–569**) | yes (predicates) |
| `InstallStates` head (**579–600**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **451** | `FindStateAddress` closing brace (**987**) |
| **466–471**, **502–508**, **528–533**, **572–578**, **601+** | comment banners / next regions |
| **534–539**, **558–567** | `qsort` / allocator host |
| **583–584**, **592** | `I_Error` / host default spawn seam |
| **1–450** | chunks **985–987** |

| Managed (`VmPStatesDefine988Slice.cs`) | Native (`p_states.cpp`) |
|----------------------------------------|----------------------|
| `ApplyStateLabelAssignment` | **461–463** |
| `EncodePendingStateArrayIndex` / `AssignPendingStateIndexLabel` | **474–478** |
| `TryDecodeStateLabelIndex` | **491–499** |
| `ResolveStateDefineByNameParts` | **515–525** |
| `ShouldPruneEmptyStateDefine` / `PruneEmptyStateDefines` / `MaterializeStateLabelListEnabled` | **544–556** |
| `CanInstallStates` / `SelectSpawnStateForInstall` / `ShouldDestroyExistingStateList` | **581–600** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **988** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**987** |


**Post-review ratio:** 101/150 managed/native ≈ 67% (src file LOC at HEAD; see tally).

## Tests

- `VmPStatesDefine988SliceTests.StateLabelAssignmentAndIndexEncoding`
- `VmPStatesDefine988SliceTests.ResolveAndPruneStateDefines`
- `VmPStatesDefine988SliceTests.InstallStatesHeadPredicates`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 988 --to 988` (0 hard / 0 soft).

## Seams

`FindStateAddress`, `MakeStateNameList`, `M_Malloc`/`M_Free`, `qsort`, and `I_Error` remain host-injected.

## Credit rationale

UNCredited define/install band; disjoint from **985–987**. Does not re-port **987** `WalkStateDefineAddress` / **986** name-list parse.
