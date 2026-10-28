# Chunk 1054 — `cluster_info_t::Reset` and `FMapInfoParser` brace/assign/number helpers from `g_mapinfo.cpp` (**451–600**). Follows **1053** (**301–450**).

## Scope

`cluster_info_t::Reset` and `FMapInfoParser` brace/assign/number helpers from `g_mapinfo.cpp` (**451–600**). Follows **1053** (**301–450**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **cluster_info_t::Reset / FMapInfoParser format helpers** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseParser1054Slice.cs`
Managed LOC (tally): 107.
Conversion commit: `06ca0591c5cd67670ec009caeff7ed4360c5abb0`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `isValid` return tail (**451–452**) | excluded — **1053** `IsLevelInfoValid` |
| `cluster_info_t::Reset` (**459–471**) | yes |
| `ParseOpenBrace` (**480–498**) | yes |
| `ParseCloseBrace` (**505–518**) | yes |
| `CheckAssign` / assign/comma helpers (**525–560**) | yes |
| `CheckNumber` / `CheckFloat` (**567–598**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **301–450** | chunk **1053** |
| **451–452** | `isValid` body — policy on **1053** slice |
| **453–458** | comment banners |
| **600+** | HCDE optional float compat (next band) |
| **833+** | chunks **1027–1036** |
| `VmMapInfoParserFormat` | uncredited brace helpers — seam only |

| Managed (`VmMapInfoParseParser1054Slice.cs`) | Native (`g_mapinfo.cpp`) |
|---------------------------------------------|---------------------------|
| `ApplyClusterInfoReset` | **459–471** |
| `ResolveFormatAfterOpenBrace` / `RequiresMustGetOpenBrace` | **480–498** |
| `TryParseCloseBrace` | **505–518** |
| `CheckAssign` / assign/comma predicates | **525–560** |
| `TryCheckNumber` / `TryCheckFloat` | **567–598** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **1054** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1053** |


**Post-review ratio:** 107/150 managed/native ≈ 71% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseParser1054SliceTests.ApplyClusterInfoReset_ClearsClusterFields`
- `VmMapInfoParseParser1054SliceTests.ResolveFormatAfterOpenBrace_DetectsNewFromBraceToken`
- `VmMapInfoParseParser1054SliceTests.ResolveFormatAfterOpenBrace_KeepsOldWhenSet`
- `VmMapInfoParseParser1054SliceTests.TryParseCloseBrace_OldFormatUngetsAndReturnsTrue`
- `VmMapInfoParseParser1054SliceTests.TryParseCloseBrace_NewFormatRequiresBraceToken`
- `VmMapInfoParseParser1054SliceTests.CheckAssign_OnlyNewFormatWithEquals`
- `VmMapInfoParseParser1054SliceTests.TryCheckNumber_NewFormatNeedsCommaThenNumber`
- `VmMapInfoParseParser1054SliceTests.TryCheckFloat_OldFormatUsesScannerFloat`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1054 --to 1054` (0 hard / 0 soft).

## Seams

`FScanner` (`CheckString`, `MustGetStringName`, `SetCMode`, `UnGet`, `CheckNumber`, `CheckFloat`) remains host-injected; uncredited `VmMapInfoParserFormat` / **1053** `IsLevelInfoValid` complete **451–452** without re-credit.

## Credit rationale

File-scoped UNCredited **`g_mapinfo.cpp`** **451–600** (+150); excludes **301–450** (**1053**). Hull ends at **`CheckFloat`** (**598**); **600+** `HCDE_MapInfo_CheckOptionalFloatCompat` deferred without padding.
