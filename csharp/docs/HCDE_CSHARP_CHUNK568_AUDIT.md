# Chunk 568 — FBehaviorContainer module load/mark/lock and SerializeModuleStates (1977–2169)

## Scope

FBehaviorContainer module load/mark/lock and SerializeModuleStates (1977–2169)

## Native ranges

- `src/playsim/p_acs.cpp` **FBehaviorContainer::LoadDefaultModules** 1977–1998 (22 lines)
- `src/playsim/p_acs.cpp` **FBehaviorContainer::LoadModule** 2000–2023 (24 lines)
- `src/playsim/p_acs.cpp` **FBehaviorContainer::CheckAllGood** 2025–2035 (11 lines)
- `src/playsim/p_acs.cpp` **FBehaviorContainer::UnloadModules** 2037–2044 (8 lines)
- `src/playsim/p_acs.cpp` **FBehaviorContainer::GetModule** 2046–2053 (8 lines)
- `src/playsim/p_acs.cpp` **FBehaviorContainer::MarkLevelVarStrings** 2055–2077 (23 lines)
- `src/playsim/p_acs.cpp` **FBehaviorContainer::LockLevelVarStrings** 2079–2101 (23 lines)
- `src/playsim/p_acs.cpp` **FBehaviorContainer::UnlockLevelVarStrings** 2103–2106 (4 lines)
- `src/playsim/p_acs.cpp` **FBehavior::MarkMapVarStrings** 2108–2115 (8 lines)
- `src/playsim/p_acs.cpp` **FBehavior::LockMapVarStrings** 2117–2124 (8 lines)
- `src/playsim/p_acs.cpp` **FBehaviorContainer::SerializeModuleStates** 2126–2169 (44 lines)

**Credited native LOC:** 183 (tally `nativeLoc` 183).

## Managed

`VmAcsBehaviorContainer.cs`
Managed LOC (tally): 114.
Conversion commit: `c9ec8cf41e349e2c8c2dafa680723878643bcfb4`.

## Padding review (2026-10-21)

| Managed | Native (`p_acs.cpp`) |
|---------|-------------------|
| `VmAcsBehaviorContainer.LoadDefaultModulesNative` | `LoadDefaultModules` 1977–1998 |
| `LoadModuleNative` | `LoadModule` 2000–2023 |
| `CheckAllGoodNative` | `CheckAllGood` 2025–2035 |
| `GetModuleIndexNative` | `GetModule` 2046–2053 |
| `MarkLevelVarStringsNative` / `LockLevelVarStringsNative` / `UnlockLevelVarStringsNative` | `MarkLevelVarStrings` 2055–2077; `LockLevelVarStrings` 2079–2101; `UnlockLevelVarStrings` 2103–2106 |
| `MarkMapVarStringsNative` / `LockMapVarStringsNative` | `FBehavior::MarkMapVarStrings` 2108–2115; `LockMapVarStrings` 2117–2124 |

**Post-review ratio:** 114/183 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmAcsBehaviorContainerTests.LoadDefaultModulesNative_LoadsExistingModules`
- `VmAcsBehaviorContainerTests.CheckAllGoodNative_FailsOnBadModule`
- `VmAcsBehaviorContainerTests.GetModuleIndexNative_ReturnsNullWhenOutOfRange`
- `VmAcsBehaviorContainerTests.MarkLevelVarStringsNative_WalksModulesAndScripts`
- `VmAcsBehaviorContainerTests.SerializeModuleStatesNative_RejectsCountMismatch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 568 --to 568` (0 hard / 0 soft).

## Seams

`FBehavior::Init`, `fileSystem`, `DLevelScript` iteration, and `GlobalACSStrings` mutation are not hosted.

## Credit rationale

Whole-function ports for ACS behavior container helpers in `p_acs.cpp`.
