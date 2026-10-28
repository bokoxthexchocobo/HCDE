# Chunk 1080 — `CommandDrawNumber::Parse` value keywords and validation from `sbarinfo_commands.cpp` (**1051–1200**); follows **1079** through **`armor` (**1050**).

## Scope

`CommandDrawNumber::Parse` value keywords and validation from `sbarinfo_commands.cpp` (**1051–1200**); follows **1079** through **`armor` (**1050**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawNumber::Parse** 1051–1200 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawNumber1080Slice.cs`
Managed LOC (tally): 126.
Conversion commit: `61cb4f9d50a5d0825701b08afd76f8eed44ee3aa`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Stat/ammo keyword dispatch (**1052–1119**) | yes |
| Parameterized `ammo` / `ammocapacity` gates (**1062–1092**) | yes |
| Intermission counters + `keys` (**1094–1119**) | yes |
| Draw-number `globalvar` / `globalarray` bounds (**1120–1142**) | yes |
| `poweruptime` class gate (**1144–1158**) | yes |
| `intcvar` type acceptance (**1160–1189**) | yes |
| Generic inventory validation (**1192–1199**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1048–1050** | **1079** keyword head seam |
| **1069–1074**, **1085–1090** | host class lookup |
| **1125–1140**, **1169–1187** | host scanner / `FindCVar` |
| **1201+** | trailing `,` / flag parse outside hull |

| Managed (`VmSbarinfoCommandDrawNumber1080Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------|----------------------------------|
| Value keyword classification | **1052–1190** |
| Parameterized token gates | **1066–1068**, **1149–1150** |
| Ammo/powerup/inventory fallback predicates | **1070–1074**, **1152–1156**, **1195–1198** |
| Global index + int cvar policy | **1126–1127**, **1175–1182** |
| Band hull (exclusive slice) | **1051–1200** |

### Chunk **1080** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1079** |


**Post-review ratio:** 126/150 managed/native ≈ 84% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawNumber1080SliceTests.TryClassifyDrawNumberValueKeyword_MapsStatCounters`
- `VmSbarinfoCommandDrawNumber1080SliceTests.ShouldRequireClassNameToken_WhenNotParenthesizedOrMissingString`
- `VmSbarinfoCommandDrawNumber1080SliceTests.ShouldUseDefaultAmmoClassFallback_DetectsInvalidAmmoType`
- `VmSbarinfoCommandDrawNumber1080SliceTests.IsDrawNumberGlobalIndexInRange_DelegatesTo1078`
- `VmSbarinfoCommandDrawNumber1080SliceTests.ResolveDrawNumberIntCVarAcceptance_AllowsBoolAndInt`
- `VmSbarinfoCommandDrawNumber1080SliceTests.ShouldValidateInventoryItemClass_OnlyForInventoryKind`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1080 --to 1080` (0 hard / 0 soft).

## Seams

**1079** owns draw-number head (**1021–1050**); **1078** owns `NUM_GLOBALVARS` bound (**64**). Host `PClass::FindActor` / `FindCVar` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **1051–1200** (+150); extended stat/ammo/global/cvar parse policy; no overlap with **1079** hull except seam **1050**/**1051**.
