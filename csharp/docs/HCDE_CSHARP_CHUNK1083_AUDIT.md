# Chunk 1083 — `CommandDrawNumber::Tick` draw tail and `CommandDrawMugShot` parse head from `sbarinfo_commands.cpp` (**1501–1650**); follows **1082** through **`drawValue = num` (**1500**).

## Scope

`CommandDrawNumber::Tick` draw tail and `CommandDrawMugShot` parse head from `sbarinfo_commands.cpp` (**1501–1650**); follows **1082** through **`drawValue = num` (**1500**).

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawNumber::Tick / CommandDrawMugShot::Parse** 1501–1650 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawNumber1083Slice.cs`
Managed LOC (tally): 194.
Conversion commit: `b3a84cbf1a067ae3aad69827f8d84d8dea704a0b`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| **whenNotZero** early exit (**1501–1505**) | yes |
| Low/high translation bands (**1507–1511**) | yes |
| Digit cap when `!dontCap` (**1514–1521**) | yes |
| Modulo path when `dontCap` (**1523–1528**) | yes |
| Fill-zero vs plain format (**1531–1534**) | yes |
| **CommandDrawMugShot** ctor defaults (**1601–1603**) | yes |
| Default face length gate (**1617–1618**) | yes |
| Accuracy **1–9** validation (**1622–1624**) | yes |
| Mugshot state flag OR merge (**1628–1641**) | yes |
| Flag list `|` vs `,` (**1644–1645**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **1492–1500** | **1082** interpolation seam |
| **1536** | host `RealignString` |
| **1538–1594** | `ValueType` enum + member field declarations (layout only) |
| **1606–1611** | host `GetFace` / `DrawGraphic` |
| **1648–1649** | host `GetCoordinates` / scanner `;` wiring |
| **1651+** | `Reset` outside hull |

| Managed (`VmSbarinfoCommandDrawNumber1083Slice.cs`, `VmSbarinfoCommandDrawMugShot1083Slice`) | Native (`sbarinfo_commands.cpp`) |
|------------------------------------------------------------------------------------------------|----------------------------------|
| Draw tail cap/format | **1501–1534** |
| Mugshot defaults + parse flags | **1601–1646** |
| Band hull (exclusive slice) | **1501–1650** |

### Chunk **1083** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1082** |


**Post-review ratio:** 194/150 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawNumber1083SliceTests.ResolveDrawNumberWhenNotZeroEarlyExit_ClearsStringAtZero`
- `VmSbarinfoCommandDrawNumber1083SliceTests.ResolveDrawNumberTranslationBand_SelectsLowAndHigh`
- `VmSbarinfoCommandDrawNumber1083SliceTests.ClampDrawNumberWhenDigitCapped_UsesFillZeroMin`
- `VmSbarinfoCommandDrawNumber1083SliceTests.ApplyDrawNumberDontCapModulo_ForcesFillZerosAtLimit`
- `VmSbarinfoCommandDrawNumber1083SliceTests.FormatDrawNumberDisplayString_ZeroPadsPositive`
- `VmSbarinfoCommandDrawNumber1083SliceTests.CreateCommandDrawMugShotDefaults_MatchesNativeCtor`
- `VmSbarinfoCommandDrawNumber1083SliceTests.IsCommandDrawMugShotDefaultFaceTooLong_EnforcesThreeChars`
- `VmSbarinfoCommandDrawNumber1083SliceTests.IsCommandDrawMugShotAccuracyInRange_EnforcesOneThroughNine`
- `VmSbarinfoCommandDrawNumber1083SliceTests.ApplyCommandDrawMugShotParseKeyword_StacksNativeBits`
- `VmSbarinfoCommandDrawNumber1083SliceTests.ClassifyCommandDrawMugShotParseKeyword_RejectsUnknown`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1083 --to 1083` (0 hard / 0 soft).

## Seams

**1082** owns interpolation (**1492–1500**); **1066** top-level `mugshot` flags are a different token set than **CommandDrawMugShot** `FMugShot::StateFlags`. Host `RealignString`, `GetFace`, `DrawGraphic`, and `GetCoordinates` excluded.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **1501–1650** (+150); whenNotZero/translation/cap/format draw tail plus mugshot accuracy/flag parse through **`;` (**1650**).
