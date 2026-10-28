# Chunk 989 — `InstallStates` cache tail, `MakeStateList`/`MakeStateDefines`, `AddStateDefines`, retarget helpers, and `ResolveGotoLabel` head from `p_states.cpp` (**601–750**); follows chunk **988** (**451–600**).

## Scope

`InstallStates` cache tail, `MakeStateList`/`MakeStateDefines`, `AddStateDefines`, retarget helpers, and `ResolveGotoLabel` head from `p_states.cpp` (**601–750**); follows chunk **988** (**451–600**).

## Native ranges

- `src/p_states.cpp` **FStateDefinitions::ResolveGotoLabel** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPStatesParser989Slice.cs`
Managed LOC (tally): 143.
Conversion commit: `8be6b422786e16da69a4070157f9d6244a98f302`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `InstallStates` default cache (**603–609**) | yes |
| `MakeStateList` / `MakeStateDefines` (**621–654**) | yes |
| `AddStateDefines` flat merge (**664–681**) | yes |
| `RetargetStatePointers` (**696–718**) | yes |
| `ResolveGotoLabel` class scope head (**734–750**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **601–602** | `CreateStateLabelList` assign (**988** seam continuation) |
| **612–619**, **656–662**, **683–694**, **726–732**, **751+** | comment banners / outside hull |
| **709**, **757–762** | host string alloc / `I_Error` |
| **1–600** | chunks **985–988** |

| Managed (`VmPStatesParser989Slice.cs`) | Native (`p_states.cpp`) |
|---------------------------------------|----------------------|
| `CacheInstalledDefaultStates` | **604–609** |
| `ImportStateLabelTree` / `ResetStateDefineSession` / `ShouldRebuildStateDefinesFromClass` | **621–654** |
| `ShouldMergeFlatStateDefine` | **668–678** |
| `RetargetIndexedDefinesInTree` / `ApplyRetarget*` | **698–716** |
| `GotoLabelHasClassScope` / `GotoClassScopeIsSuper` / `ExtractGotoLabelAfterScope` | **743–754** |
| Band hull (exclusive slice) | **601–750** |

### Chunk **989** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**988** |


**Post-review ratio:** 143/150 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmPStatesParser989SliceTests.InstallCacheAndImport`
- `VmPStatesParser989SliceTests.RetargetIndexedDefines`
- `VmPStatesParser989SliceTests.ResolveGotoLabelHead`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 989 --to 989` (0 hard / 0 soft).

## Seams

`CreateStateLabelList`, `FindState`, `FxAlloc.Strdup`, `I_Error`, and `strstr` host calls remain injected.

## Credit rationale

UNCredited parser/dehacked band; disjoint from **985–988**. Does not re-port **988** install head (**579–600**).
