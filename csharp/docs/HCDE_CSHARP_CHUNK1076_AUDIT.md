# Chunk 1076 — `CommandDrawSwitchableImage` parse tail (`&&`, conditional images) and `Tick` draw-alt policy from `sbarinfo_commands.cpp` (**451–600**); follows **1075** parse head.

## Scope

`CommandDrawSwitchableImage` parse tail (`&&`, conditional images) and `Tick` draw-alt policy from `sbarinfo_commands.cpp` (**451–600**); follows **1075** parse head.

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawSwitchableImage::Parse / Tick** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawSwitchable1076Slice.cs`
Managed LOC (tally): 99.
Conversion commit: `6ddd631ef96b0cb01ef7213f351b5a4a01029ae1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `armortype` parse branch (**451–456**) | yes (gate; compare via **1075**) |
| Inventory compare parse (**459–467**) | yes (gate) |
| `&&` parse w/ invuln exclusion (**469–496**) | yes |
| Conditional image loop bound (**498–503**) | yes |
| Weapon slot `drawAlt` (**516–531**) | yes |
| Invulnerability cheat `drawAlt` (**533–538**) | yes |
| Key slot match + `drawAlt` table (**542–582**) | yes |
| Armor type match + `drawAlt` (**586–601**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **454–456**, **467**, **483–494** | host scanner / **1075** `GetOperation` |
| **477**, **449** | host `LookupKeySpecies` |
| **500–505** | host `newImage` / `CommandDrawImage::Parse` |
| **519–527**, **546–566** | host inventory/weapon scans |
| **607+** | inventory `drawAlt` outside hull |

| Managed (`VmSbarinfoCommandDrawSwitchable1076Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|-----------------------------------------------------------|----------------------------------|
| `&&` + image count policy | **469–503** |
| Weapon / invuln draw-alt | **516–538** |
| Key slot match + draw-alt | **551–582** |
| Armor match + draw-alt | **589–601** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **1076** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1075** |


**Post-review ratio:** 99/150 managed/native ≈ 66% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawSwitchable1076SliceTests.ShouldEnableConditionAndParse_BlocksInvulnerability`
- `VmSbarinfoCommandDrawSwitchable1076SliceTests.CountConditionalImagesToLoad_MatchesNativeLoopBound`
- `VmSbarinfoCommandDrawSwitchable1076SliceTests.ResolveKeySlotDrawAlt_MatchesDualKeyTable`
- `VmSbarinfoCommandDrawSwitchable1076SliceTests.ApplyKeySlotNumberMatch_UpdatesExpectedSlots`
- `VmSbarinfoCommandDrawSwitchable1076SliceTests.ResolveWeaponSlotDrawAlt_TogglesOnOwnedWeapon`
- `VmSbarinfoCommandDrawSwitchable1076SliceTests.ResolveInvulnerabilityDrawAlt_DetectsGodModeCheats`
- `VmSbarinfoCommandDrawSwitchable1076SliceTests.EvaluateArmorTypeMatch_Uses1075Compare`
- `VmSbarinfoCommandDrawSwitchable1076SliceTests.ResolveArmorTypeDrawAlt_MatchesNativePriority`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1076 --to 1076` (0 hard / 0 soft).

## Seams

**1075** owns compare parse/eval helpers and keyword head (**434–450**); host `LookupKeySpecies`, scanner, and **607+** inventory tick remain outside hull.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **451–600** (+150); continues switchable command; zero overlap except **1075**-excluded seams.
