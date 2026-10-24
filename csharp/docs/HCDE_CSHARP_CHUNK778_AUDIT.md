# Chunk 778 — `FPlayerClass::CheckSkin`, display-name helper, player-class validation/setup, `player_t` destructor, `CopyFrom`, and `PropagateMark` (`p_user.cpp` 632–838). Excludes `CCMD(playerclasses)` 704–712 and `FPlayerClass` ctor/dtor 622–630.

## Scope

`FPlayerClass::CheckSkin`, display-name helper, player-class validation/setup, `player_t` destructor, `CopyFrom`, and `PropagateMark` (`p_user.cpp` 632–838). Excludes `CCMD(playerclasses)` 704–712 and `FPlayerClass` ctor/dtor 622–630.

## Native ranges

- `src/playsim/p_user.cpp` **FPlayerClass::CheckSkin** 632–641 (10 lines)
- `src/playsim/p_user.cpp` **DEFINE_ACTION_FUNCTION(FPlayerClass, CheckSkin)** 643–648 (6 lines)
- `src/playsim/p_user.cpp` **GetPrintableDisplayName** 656–661 (6 lines)
- `src/playsim/p_user.cpp` **ValidatePlayerClass** 663–681 (19 lines)
- `src/playsim/p_user.cpp` **SetupPlayerClasses** 683–702 (20 lines)
- `src/playsim/p_user.cpp` **player_t::~player_t** 718–721 (4 lines)
- `src/playsim/p_user.cpp` **player_t::CopyFrom** 723–818 (96 lines)
- `src/playsim/p_user.cpp` **player_t::PropagateMark** 820–838 (19 lines)

**Credited native LOC:** 180 (tally `nativeLoc` 180).

## Managed

`VmPUserPlayerClass.cs`
Managed LOC (tally): 348.
Conversion commit: ``.

## Padding review (2026-10-21)

| Managed | Native (`p_user.cpp`) |
|---------|-------------------|
| `CheckSkin` / `CheckSkinAction` | `FPlayerClass::CheckSkin` **632–641**, `DEFINE_ACTION_FUNCTION(FPlayerClass, CheckSkin)` **643–648** |
| `GetPrintableDisplayName` | **656–661** |
| `ValidatePlayerClass` | **663–681** |
| `SetupPlayerClasses` | **683–702** |
| `DestroyPlayer` | `player_t::~player_t` **718–721** |
| `CopyFrom` | `player_t::CopyFrom` **723–818** |
| `PropagateMark` | `player_t::PropagateMark` **820–838** |


**Post-review ratio:** 348/180 managed/native ≈ 193% (src file LOC at HEAD; see tally).

## Tests

- `VmPUserPlayerClassTests.CheckSkin_FindsRegisteredSkin`
- `VmPUserPlayerClassTests.CheckSkinAction_ReturnsBool`
- `VmPUserPlayerClassTests.GetPrintableDisplayName_DelegatesToClass`
- `VmPUserPlayerClassTests.ValidatePlayerClass_RejectsNull`
- `VmPUserPlayerClassTests.ValidatePlayerClass_RejectsNonPawn`
- `VmPUserPlayerClassTests.ValidatePlayerClass_RejectsEmptyDisplayName`
- `VmPUserPlayerClassTests.ValidatePlayerClass_AcceptsValid`
- `VmPUserPlayerClassTests.SetupPlayerClasses_AppliesNomenuFlag`
- `VmPUserPlayerClassTests.SetupPlayerClasses_SkipsInvalid`
- `VmPUserPlayerClassTests.DestroyPlayer_CallsDestroyPsprites`
- `VmPUserPlayerClassTests.CopyFrom_CopiesScalarsAndFrags`
- `VmPUserPlayerClassTests.CopyFrom_TransfersPsprites`
- `VmPUserPlayerClassTests.PropagateMark_MarksPendingWeapon`
- `VmPUserPlayerClassTests.PropagateMark_SkipsWpNochange`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 778 --to 778` (0 hard / 0 soft).

## Seams

`PClass::FindActor`, `GetDefaultByType`, `DestroyPSprites`, `GC::Mark`, `Printf`, `DEFINE_ACTION_FUNCTION` VM dispatch, and `userinfo_t` remain native.

## Credit rationale

Faithful whole-function ports for player-class registry and player snapshot copy/GC bands; replaces mistaken `a_specialspot.cpp` overlap in chunk778.json.
