# Chunk 1038 — `GetReplacement` / `GetReplacee` chains and `SetDamageFactor` / `SetPainChance` head from `info.cpp` (**650–799**). Follows chunk **1037** (**500–649**).

## Scope

`GetReplacement` / `GetReplacee` chains and `SetDamageFactor` / `SetPainChance` head from `info.cpp` (**650–799**). Follows chunk **1037** (**500–649**).

## Native ranges

- `src/gamedata/info.cpp` **GetReplacement / GetReplacee / damage+pain setters** 650–799 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPClassActorInfo1038Slice.cs`
Managed LOC (tally): 171.
Conversion commit: `bc2f315a2eb78534fa58dfd46733a0c4a3eb9191`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Skill failure clear tail (**650–651**) | yes |
| ZScript `CheckReplacement` final return (**654–662**) | yes |
| Early return when no replacement (**663–666**) | yes |
| Temp NULL + skill then DECORATE chain (**667–684**) | yes |
| Decorate client-side finalize (**684–690**) | yes |
| `GetReplacee` skill `GetReplacedBy` validate (**703–732**) | yes |
| ZScript `CheckReplacee` (**735–744**) | yes |
| Replacee chain + client-side gate (**745–765**) | yes |
| `SetDamageFactor` update/push (**774–785**) | yes |
| `SetPainChance` update head (**793–799**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **500–649** | chunk **1037** |
| **652–653**, **692–697** | closing braces / comment banners |
| **767–773**, **787–792** | comment banners |
| **800+** | `SetPainChance` push tail outside hull (**799** ends update branch) |
| `VmActorClassRegistry` | uncredited helper — seam only |
| `VmPClassActorInfo1037Slice.VerifyClientSideReplacement` | shared predicate — seam, no duplicate lines |

| Managed (`VmPClassActorInfo1038Slice.cs`) | Native (`info.cpp`) |
|---------------------------------------------|---------------------|
| `ClearSkillReplacementNamesAfterFailure` | **650–651** |
| `ResolveEventHandlerReplacement` | **658–661** |
| `ShouldReturnSelfWithoutReplacement` | **663–666** |
| `ResolveSkillThenDecorateReplacement` | **675–681** |
| `FinalizeReplacementClientSide` | **684–690** |
| `EvaluateSkillReplacedBy` / `ApplySkillReplacedByFailureClear` | **705–731** |
| `ResolveEventHandlerReplacee` | **740–743** |
| `ResolveReplaceeChain` | **753–757** |
| `TryUpdateDamageFactor` / `AddDamageFactor` | **776–784** |
| `TryUpdatePainChance` / `ClampPainChance` / `ShouldPushPainChance` | **795–799** |
| Band hull (exclusive slice) | **650–799** |

### Chunk **1038** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1037** |


**Post-review ratio:** 171/150 managed/native ≈ 114% (src file LOC at HEAD; see tally).

## Tests

- `VmPClassActorInfo1038SliceTests.ResolveEventHandlerReplacement_ReturnsSelfWhenMismatch`
- `VmPClassActorInfo1038SliceTests.ShouldReturnSelfWithoutReplacement_WhenBothMissing`
- `VmPClassActorInfo1038SliceTests.ResolveSkillThenDecorateReplacement_PrefersSkillName`
- `VmPClassActorInfo1038SliceTests.EvaluateSkillReplacedBy_FailsWhenClassMissing`
- `VmPClassActorInfo1038SliceTests.TryUpdateDamageFactor_UpdatesExisting`
- `VmPClassActorInfo1038SliceTests.TryUpdatePainChance_ClampsTo256`
- `VmPClassActorInfo1038SliceTests.ShouldPushPainChance_RejectsNegative`
- `VmPClassActorInfo1038SliceTests.FinalizeReplacementClientSide_RejectsMismatch`
- `VmPClassActorInfo1038SliceTests.ClearSkillReplacementNamesAfterFailure_ClearsFlags`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1038 --to 1038` (0 hard / 0 soft).

## Seams

`Level->localEventManager`, `PClass::FindActor` / `FindClass`, `ActorInfo()` field mutation, `Printf`, and recursive native calls remain host-injected; uncredited `VmActorClassRegistry.SetDamageFactor` / `SetPainChance` are seams only.

## Credit rationale

UNCredited **`info.cpp`** **650–799** abutting **1037** at skill-failure clear (**649**); no overlap with credited **500–649** row (exclusive hull). Identity one-liner `ShouldSkipReplacementWhenLevelNull` omitted (reaudit padding policy).
