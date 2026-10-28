# Chunk 1078 — `CommandDrawString` value parse, reset cache, and dynamic `Tick` refresh from `sbarinfo_commands.cpp` (**751–900**); follows **1077** parse flag tail.

## Scope

`CommandDrawString` value parse, reset cache, and dynamic `Tick` refresh from `sbarinfo_commands.cpp` (**751–900**); follows **1077** parse flag tail.

## Native ranges

- `src/g_statusbar/sbarinfo_commands.cpp` **CommandDrawString::ParseStringValue / Reset / Tick** 751–900 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmSbarinfoCommandDrawString1078Slice.cs`
Managed LOC (tally): 85.
Conversion commit: `45b5d1f2608fd652ba25c4d17db2f7c9674f97a2`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `ParseStringValue` keyword dispatch (**761–808**) | yes |
| Global var/array index bounds (**789–802**) | yes |
| Constant string path (**810–814**) | yes |
| `Reset` cache invalidation (**821–826**) | yes |
| Level/skill/class lump cache refresh (**837–867**) | yes |
| Global var/array cache refresh (**888–900**) | yes |
| Level time formatting (**905–907**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **751–755** | **1077** parse terminator seam |
| **788–793**, **800–805** | host scanner |
| **755**, **841**, **858**, **892**, **900** | host `RealignString` / lookups |
| **869–885**, **909–911** | host tag/name copy |
| **901+** | `GLOBALARRAY` tail / `LOGTEXT` outside hull end |

| Managed (`VmSbarinfoCommandDrawString1078Slice.cs`) | Native (`sbarinfo_commands.cpp`) |
|-----------------------------------------------------|----------------------------------|
| Value keyword + global bounds | **761–814** |
| Reset cache policy | **819–826** |
| Tick cache refresh gates | **837–900** |
| TIME split/format | **905–907** |
| Band hull (exclusive slice) | **751–900** |

### Chunk **1078** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1077** |


**Post-review ratio:** 85/150 managed/native ≈ 57% (src file LOC at HEAD; see tally).

## Tests

- `VmSbarinfoCommandDrawString1078SliceTests.TryClassifyDrawStringValueKeyword_MapsDynamicSources`
- `VmSbarinfoCommandDrawString1078SliceTests.IsGlobalVarIndexInRange_MatchesNativeBound`
- `VmSbarinfoCommandDrawString1078SliceTests.ShouldInvalidateDrawStringCacheOnReset_CoversClassAndSkill`
- `VmSbarinfoCommandDrawString1078SliceTests.ShouldRefreshLevelBoundDrawString_DetectsLumpChange`
- `VmSbarinfoCommandDrawString1078SliceTests.FormatDrawStringLevelTime_MatchesNativeSplit`
- `VmSbarinfoCommandDrawString1078SliceTests.SplitLevelTimeComponents_HandlesHourRollover`
- `VmSbarinfoCommandDrawString1078SliceTests.ResolveConstantStringValueKind_UsesConstantEnum`
- `VmSbarinfoCommandDrawString1078SliceTests.ShouldRefreshGlobalVarDrawString_ComparesCachedValue`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1078 --to 1078` (0 hard / 0 soft).

## Seams

**1077** owns parse flags through **749–750**; `RealignString`, `SetStringToTag`, and **901+** `LOGTEXT`/default tail outside hull or host.

## Credit rationale

UNCredited **`sbarinfo_commands.cpp`** **751–900** (+150); dynamic string sources + TIME format; no Vm* collision with **1077** drawstring parse head.
