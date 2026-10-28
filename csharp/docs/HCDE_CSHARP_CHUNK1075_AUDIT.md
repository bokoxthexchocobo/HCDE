# Chunk 1075 — `CommandDrawSwitchableImage` compare operators and condition parse head from `sbarinfo_commands.cpp` (**301–450**); follows **1074** `GetIcon` tail seam.

## Scope

`CommandDrawSwitchableImage` compare operators and condition parse head from `sbarinfo_commands.cpp` (**301–450**); follows **1074** `GetIcon` tail seam.

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawSwitchableImage::GetOperation / Parse** 301–450 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawSwitchable1075Slice.cs`
Managed LOC (tally): 98.
Conversion commit: `b1177f5114e58555ebbee6b6af052646ca11915d`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `GetOperation` token dispatch (**364–383**) | yes |
| Default `GREATER` / value 0 (**377–380**) | yes |
| `EvaluateOperation` switch (**387–402**) | yes |
| Switchable ctor field init (**424–426**) | yes |
| `weaponslot` / `invulnerable` / `keyslot` parse (**434–449**) | yes |
| Inventory string-const path gate (**459** predicate via **430–432**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **301–304** | **1074** `ApplyInventoryIconSpawnScale` seam |
| **306–307**, **405–418** | host `TexMan` / actor scan |
| **309–345** | `ImageType` enum + member fields |
| **428–431**, **437–449** | host scanner tokens |
| **451+** | `armortype` parse outside hull |

| Managed (`VmSbarinfoCommandDrawSwitchable1075Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|-----------------------------------------------------------|----------------------------------|
| Compare operator parse + default | **362–383** |
| `EvaluateOperation` | **385–403** |
| Ctor sentinel init | **424–426** |
| Condition keyword classification | **434–447** |
| Inventory / slot parse gates | **428–459** |
| Band hull (exclusive slice) | **301–450** |

### Chunk **1075** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1074** |


**Post-review ratio:** 98/150 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawSwitchable1075SliceTests.ResolveCompareOperationFromTokens_DefaultsToGreaterZero`
- `VmSbarinfoCommandDrawSwitchable1075SliceTests.EvaluateSwitchableCompare_CoversNativeOperators`
- `VmSbarinfoCommandDrawSwitchable1075SliceTests.TryClassifySwitchableConditionKeyword_MapsParseHeadKeywords`
- `VmSbarinfoCommandDrawSwitchable1075SliceTests.InitializeSwitchableImageFields_MatchesNativeCtorSentinels`
- `VmSbarinfoCommandDrawSwitchable1075SliceTests.ShouldParseInventoryConditionString_DetectsStringConstPath`
- `VmSbarinfoCommandDrawSwitchable1075SliceTests.ShouldParseWeaponSlotIndex_OnlyForWeaponSlotCondition`
- `VmSbarinfoCommandDrawSwitchable1075SliceTests.ShouldParseArmorTypeIdentifier_DetectsArmorTypeBranch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1075 --to 1075` (0 hard / 0 soft).

## Seams

**1074** owns spawn-scale tail (**300–304**); `LookupKeySpecies`, `FScanner`, and **451+** `armortype` body remain outside hull or host.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **301–450** (+150); introduces switchable-image policy; no tally overlap with **1074** except excluded seam.
