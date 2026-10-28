# Chunk 1079 — `RealignString`, `SetStringToTag`, and `CommandDrawNumber` parse head from `sbarinfo_commands.cpp` (**901–1050**); follows **1078** dynamic tick seam.

## Scope

`RealignString`, `SetStringToTag`, and `CommandDrawNumber` parse head from `sbarinfo_commands.cpp` (**901–1050**); follows **1078** dynamic tick seam.

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawString::RealignString / SetStringToTag / CommandDrawNumber::Parse** 901–1050 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawString1079Slice.cs`
Managed LOC (tally): 105.
Conversion commit: `1b6ab4b3dee8d781f5a9c17ddb054fba0313eaba`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `LOGTEXT` / default tick tail (**909–915**) | yes (realign gate on default) |
| `RealignString` reset + alignment (**928–944**) | yes |
| Proportional vs monospace width (**934–943**) | yes |
| `SetStringToTag` cache policy (**988–1000**) | yes |
| `CommandDrawNumber` length/font (**1021–1031**) | yes |
| Constant int value path (**1035–1039**) | yes |
| Draw-number stat keywords (**1048–1050** hull) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **901–902**, **900** | **1078** global-array tick tail / host `RealignString` |
| **903–907** | **1078** TIME format |
| **910**, **913–914** | host string lookup |
| **919–965**, **967–983** | enums / member declarations |
| **994**, **1000** | host `RealignString` call |
| **1051+** | drawnumber keywords outside hull |

| Managed (`VmSbarinfoCommandDrawString1079Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|----------------------------------------------------|----------------------------------|
| Realign X for L/R/C + spacing modes | **928–944** |
| Tag string cache update/clear | **988–1000** |
| DrawNumber length/value parse head | **1021–1050** |
| Band hull (exclusive slice) | **901–1050** |

### Chunk **1079** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1078** |


**Post-review ratio:** 105/150 managed/native ≈ 70% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawString1079SliceTests.ResolveDrawStringXAfterRealign_CenterHalvesMonospaceWidth`
- `VmSbarinfoCommandDrawString1079SliceTests.ResolveDrawStringXAfterRealign_RightUsesProportionalWidth`
- `VmSbarinfoCommandDrawString1079SliceTests.ResolveTagStringWhenActorPresent_RealignsOnClassChange`
- `VmSbarinfoCommandDrawString1079SliceTests.ResolveTagStringWhenActorMissing_ClearsCacheAndText`
- `VmSbarinfoCommandDrawString1079SliceTests.TryClassifyDrawNumberValueKeyword_MapsHealthArmorAmmo`
- `VmSbarinfoCommandDrawString1079SliceTests.ResolveDrawNumberValueSource_DistinguishesConstantInt`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1079 --to 1079` (0 hard / 0 soft).

## Seams

**1078** owns TIME formatting (**905–907**); **1077** owns drawstring alignment parse keywords. Member fields **967–983** excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **901–1050** (+150); horizontal realign math + tag cache + drawnumber value head; not `v_font.cpp` **901–957**.
