# Chunk 1037 — `PClassActor::InitializeDefaults` tail, `RegisterIDs`, and `GetReplacement` skill-validation head from `info.cpp` (**500–649**).

## Scope

`PClassActor::InitializeDefaults` tail, `RegisterIDs`, and `GetReplacement` skill-validation head from `info.cpp` (**500–649**).

## Native ranges

- `src/gamedata/info.cpp` **PClassActor defaults / RegisterIDs / GetReplacement head** 500–649 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPClassActorInfo1037Slice.cs`
Managed LOC (tally): 126.
Conversion commit: `6afc71dbedce6b21d375d477a62637011c46d17b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Parent defaults `memcpy` + zero extend (**500–504**) | yes |
| Transferrable `ObjectFlags` copy (**506**) | yes |
| No-parent defaults zero (**508–511**) | yes |
| Meta malloc + parent copy + zero extend (**514–530**) | yes |
| `RegisterIDs` conversation id (**567–574**) | yes (policy) |
| Game-filtered spawn / doom ed (**575–603**) | yes |
| Decorate doom-ed duplicate warn (**589–593**) | yes |
| `VerifyClientSideReplacement` (**613–616**) | yes |
| Skill replacement validate + clear (**622–649**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **483–499** | `InitializeDefaults` prologue / GC unlink outside hull |
| **536–537** | `PClass::InitializeDefaults` host call |
| **539–566** | comment + `FindActor` host |
| **605** | function close only |
| **650+** | ZScript replacement chain outside hull (**649** ends failure clear) |
| `VmActorClassRegistry` | uncredited StaticInit helper — seam only |

| Managed (`VmPClassActorInfo1037Slice.cs`) | Native (`info.cpp`) |
|---------------------------------------------|---------------------|
| `CopyParentDefaultsBlock` / `ExtendedDefaultsZeroLength` | **500–504** |
| `TransferrableObjectFlags` | **506** |
| `ZeroDefaultsTail` | **510–511** |
| `CopyMetaBlock` | **518–530** |
| `ShouldRegisterConversationId` | **567–574** |
| `ShouldRegisterSpawnOrDoomEd` / spawn + doom ed predicates | **575–603** |
| `CreateDecorateDoomEdEntry` / `IsDuplicateDecorateDoomEdEntry` | **589–598** |
| `VerifyClientSideReplacement` | **613–616** |
| `EvaluateSkillReplacement` / `ApplySkillReplacementFailureClear` | **622–649** |
| Band hull (exclusive slice) | **500–649** |

### Chunk **1037** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1036** |


**Post-review ratio:** 126/150 managed/native ≈ 84% (src file LOC at HEAD; see tally).

## Tests

- `VmPClassActorInfo1037SliceTests.CopyParentDefaultsBlock_CopiesAndZeroExtends`
- `VmPClassActorInfo1037SliceTests.TransferrableObjectFlags_MasksToTransferBits`
- `VmPClassActorInfo1037SliceTests.ShouldRegisterSpawnOrDoomEd_RespectsGameFilter`
- `VmPClassActorInfo1037SliceTests.CreateDecorateDoomEdEntry_UsesSpecialMinusTwo`
- `VmPClassActorInfo1037SliceTests.EvaluateSkillReplacement_FailsWhenClassMissing`
- `VmPClassActorInfo1037SliceTests.EvaluateSkillReplacement_FailsOnClientSideMismatch`
- `VmPClassActorInfo1037SliceTests.ApplySkillReplacementFailureClear_ClearsLookSkill`
- `VmPClassActorInfo1037SliceTests.IsDuplicateDecorateDoomEdEntry_DetectsDecorateMarker`
- `VmPClassActorInfo1037SliceTests.CopyMetaBlock_ZeroFillsExtension`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1037 --to 1037` (0 hard / 0 soft).

## Seams

`ConstructNative`, `GC::Root`, `M_Malloc`, `PClass::FindActor`, `Printf`, `StrifeTypes` / `SpawnableThings` / `DoomEdMap` tables, and `AllSkills` mutation remain host-injected; uncredited `VmActorClassRegistry` is not re-credited.

## Credit rationale

First tally-clean **150**-line hull on **`info.cpp`** at **500–649** with zero conversion overlap (gamedata pivot off deferred `g_mapinfo` gaps).
