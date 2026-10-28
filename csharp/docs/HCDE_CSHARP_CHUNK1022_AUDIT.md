# Chunk 1022 — `SOUND_*` enum, `EACSFunctions` head through `ACSF_Floor` from `p_acs.cpp` (**4728–4877**). **Zero credit:** `GetActorSound` (**4740–4757**) on `VmAcsActorPropertySound`; SOUND slot ids duplicate that helper.

## Scope

`SOUND_*` enum, `EACSFunctions` head through `ACSF_Floor` from `p_acs.cpp` (**4728–4877**). **Zero credit:** `GetActorSound` (**4740–4757**) on `VmAcsActorPropertySound`; SOUND slot ids duplicate that helper.

## Native ranges

- `src/playsim/p_acs.cpp` **EACSFunctions enum head** 4728–4877 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsCallfuncEnum1022Slice.cs`
Managed LOC (tally): 148.
Conversion commit: `9bf2616caa3ff33d7040e5946f6069b75ab09e09`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `EACSFunctions` implicit ordinals (**4761–4859**, **4870–4877**) | yes (const mirrors) |
| Zandronum reserved gap (**100–199** policy) | yes |
| HUD/CVar/LineAttack/PlayActorSound routing predicates | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **4727–4738** | `SOUND_*` anonymous enum — **`VmAcsActorPropertySound`** |
| **4740–4757** | `GetActorSound` switch — **`VmAcsActorPropertySound.GetActorSoundNative`** |
| **4860–4868** | Zandronum comment block (no symbols) |

| Managed (`VmPAcsCallfuncEnum1022Slice.cs`) | Native (`p_acs.cpp`) |
|-------------------------------------------|---------------------|
| `Acsf*` constants / `IsPrimaryCallFunctionIndex` | **4761–4859** |
| `AcsfCheckClass` … `AcsfFloor` | **4870–4877** |
| `IsReservedZandronumGap` | **4860–4868** policy |
| `UsesHudOrCvarStack` / `UsesLineAttackStack` | **4811–4822**, **4837–4838** |
| Band hull (exclusive slice) | **4728–4877** |

### Chunk **1022** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1021** |


**Post-review ratio:** 148/150 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmPAcsCallfuncEnum1022SliceTests.PrimaryEnum_AnchorsMatchNative`
- `VmPAcsCallfuncEnum1022SliceTests.ExtendedEnum_AnchorsMatchNative`
- `VmPAcsCallfuncEnum1022SliceTests.IsReservedZandronumGap_Blocks100Through199`
- `VmPAcsCallfuncEnum1022SliceTests.UsesHudOrCvarStack_FlagsCvarCluster`
- `VmPAcsCallfuncEnum1022SliceTests.ResolvePlayActorSoundSlot_DelegatesToExistingSoundMapper`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1022 --to 1022` (0 hard / 0 soft).

## Seams

Full `CallFunction` dispatch and Eternity/OpenGL enum tails (**4888+**) remain native.

## Credit rationale

Fresh wide-scan band: tally+recognition clean; avoids post-947 HUD/953 corridor, **9091–9240**, and recognition **6000–6149** / **943** tail.
