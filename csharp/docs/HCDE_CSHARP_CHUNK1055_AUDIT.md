# Chunk 1055 — Optional float compat, `SkipToNext`, EOF guard, `ParseLookupName`, and lump token from `g_mapinfo.cpp` (**601–750**). Follows **1054** (**451–600**).

## Scope

Optional float compat, `SkipToNext`, EOF guard, `ParseLookupName`, and lump token from `g_mapinfo.cpp` (**601–750**). Follows **1054** (**451–600**).

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **HCDE_MapInfo_CheckOptionalFloatCompat / ParseLookupName** 601–750 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmMapInfoParseMid1055Slice.cs`
Managed LOC (tally): 113.
Conversion commit: `802354ddf50235e49e045eb8072f9803462decdc`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `HCDE_MapInfo_CheckOptionalFloatCompat` (**600–634**) | yes |
| `SkipToNext` (**642–652**) | yes |
| `CheckEndOfFile` (**661–667**) | yes |
| `ParseLookupName` (**676–735**) | yes |
| `ParseLumpOrTextureName` (**742–746**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **451–600** | chunk **1054** |
| **636–641**, **669–675**, **737–741**, **748–753** | comment banners |
| **754+** | `ParseExitText` / ParseExit* denylist corridor |
| **833+** | chunks **1027–1036** |
| `VmMapInfoOptionalFloatCompat` / `VmMapInfoParseLookupName` | uncredited helpers — seam only |

| Managed (`VmMapInfoParseMid1055Slice.cs`) | Native (`g_mapinfo.cpp`) |
|-------------------------------------------|---------------------------|
| `TryResolveOptionalFloatCompat` / warning gate | **600–634** |
| `CountSkipToNextTokens` | **642–652** |
| `ShouldScriptErrorUnexpectedEof` | **661–667** |
| lookup / multiline / trailing-comma compat | **676–735** |
| `AssignMustGetString` | **742–746** |
| Band hull (exclusive slice) | **601–750** |

### Chunk **1055** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1054** |


**Post-review ratio:** 113/150 managed/native ≈ 75% (src file LOC at HEAD; see tally).

## Tests

- `VmMapInfoParseMid1055SliceTests.TryResolveOptionalFloatCompat_AcceptsLegacyNumber`
- `VmMapInfoParseMid1055SliceTests.CountSkipToNextTokens_CountsCommaSeparatedValues`
- `VmMapInfoParseMid1055SliceTests.ShouldScriptErrorUnexpectedEof_OnlyNewFormatAtEnd`
- `VmMapInfoParseMid1055SliceTests.TryParseDollarLookupPrefix_StripsSigil`
- `VmMapInfoParseMid1055SliceTests.TrimTrailingNewlineFromMultiline_RemovesFinalNewline`
- `VmMapInfoParseMid1055SliceTests.ShouldStopMultilineOnTrailingCommaCompat_DetectsCloseBraceAfterComma`
- `VmMapInfoParseMid1055SliceTests.TokenIsLookupKeyword_IsCaseInsensitive`
- `VmMapInfoParseMid1055SliceTests.AssignMustGetString_CoalescesNull`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1055 --to 1055` (0 hard / 0 soft).

## Seams

`FScanner`, `Printf`, `ScriptError`, `ParseComma`, and `ParseExitText` (**754+**) remain host-injected; uncredited `VmMapInfoOptionalFloatCompat` / `VmMapInfoParseLookupName` are not re-credited.

## Credit rationale

File-scoped UNCredited **`g_mapinfo.cpp`** **601–750** (+150); excludes **451–600** (**1054**). Hull stops before **`ParseExitText`** (**754+** denylist adjacency); **ParseLumpOrTextureName** (**742–746**) inside band.
