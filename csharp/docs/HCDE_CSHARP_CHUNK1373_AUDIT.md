# Chunk 1373 — Dedicated-server runtime fields, argument parsing, and init through SetAuthorityState from hcde_servermode.cpp (1-218).

## Scope

Dedicated-server runtime fields, argument parsing, and init through SetAuthorityState from hcde_servermode.cpp (1-218).

## Native ranges

- `src/common/engine/hcde_servermode.cpp` **HcdeServerModeInit** 1–218 (218 lines)

**Credited native LOC:** 218 (tally `nativeLoc` 218).

## Managed

`VmServerMode1373Slice.cs`
Managed LOC (tally): 278.
Conversion commit: `56d841b035b4c88a2cb8b7c9dcf7a07f96526571`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| File header, includes, and `FArg` declarations (**1–32**) | yes |
| `FHCDEServerRuntime` defaults and the `Runtime` instance (**33–65**) | yes |
| `IsValueArg` through `YesNo` (**67–128**) | yes |
| `HCDE_ServerMode_InitFromArgs` (**131–172**) | yes |
| `SetSelectedIWAD` through `SetAuthorityState` (**174–217**) | yes |
| Blank line closing the band (**218**) | yes |
| Band hull (**1–218**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| hcde_servermode.cpp **219–395** | accessors, guards, and diagnostics, chunk 640 |
| m_argv.cpp `CheckParm` / `CheckValue` | callee, not this hole |

`HCDE_DEDICATED_SERVER` is the compile flag stored on the slice. It is applied only when init runs, and it stays false until then. A null `Args` pointer sets that flag and returns without `Initialized`, so a later call can still parse. `CheckParm` starts at argument 1 and compares names without case. `IsValueArg` rejects a null pointer and a leading `-` or `+`. An empty string is a value argument. `CheckValue` still returns null for an empty string. `ReadIntValue` is `strtol` base 10: the whole token must be consumed, `010` is 10, and a value outside `int` or an out-of-range `long` is 0. A missing flag still returns 0 from `CheckParm`, and that integer read then inspects argument 1. `+map` wins over `-warp`. Warp keeps a second value argument and separates the two with one space. Join text is `-join`, then `-joindedicated`, then `-dedicatedjoin`. The init log is `Infof` category `serv` with the source format, and `TextOr` substitutes `default` for an empty map. The dedicated executable fatals with the source sentence only when arguments exist and `-server` is absent. `YesNo` is declared in this band; its call sites are the later diagnostics. `SetNetworkDetails` logs `?` for an empty flow. `SetMasterState` logs `%zu`. `SetAuthorityState` logs `Markf` and substitutes `unknown` for an empty name. The function ends at line 217. Line 218 is the blank line in the band.

| Managed (`VmServerMode1373Slice.cs`) | Native (`hcde_servermode.cpp`) |
|--------------------------------------|--------------------------------|
| Runtime init through `SetAuthorityState` | **1–218** |
| Band hull (exclusive slice) | **1–218** |

### Chunk **1373** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **218** |
| **Net headline** | **+218** vs the prior chunk |


**Post-review ratio:** 278/218 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmServerMode1373SliceTests.InitFromArgsParsesStrictValuesAndFatals`
- `VmServerMode1373SliceTests.SettersRecordNetworkMasterAndAuthority`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1373 --to 1373` (0 hard / 0 soft).

## Seams

CheckParm, CheckValue, and strtol base 10 are the m_argv and libc callees. DebugTrace and I_FatalError are recorded on the slice. Chunk 640 already owns hcde_servermode.cpp 219-395, including SetAuthoritySettingsController.

## Credit rationale

UNCredited hcde_servermode.cpp 1-218 (+218). A full tally line scan, including recognition ranges, ties this hole with p_switch.cpp 235-452. Both are 218 lines. Path order takes the engine file. The next accessor at line 219 stays chunk 640. Gaps longer than 400 lines stay whole and are not sliced.
