# Chunk 1217 — XLAT keyword search and translator load from parse_xlat.cpp (1-167).

## Scope

XLAT keyword search and translator load from parse_xlat.cpp (1-167).

## Native ranges

- `src/gamedata/xlat/parse_xlat.cpp` **XlatParseContext::FindToken** 1–167 (167 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmXlat1217Slice.cs`
Managed LOC (tally): 139.
Conversion commit: `e65b23945b1f7fdc2a180ced629fe85fecfb602c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FindToken (**99–135**) | yes |
| P_LoadTranslator (**150–167**) | yes |
| Parser data and context (**45–139**) | yes |
| Band hull (**1–167**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| xlat_parser.c include (**141**) | lemon grammar lives outside this band |

Keyword compare is ASCII case-insensitive. The token lineid stores tag code 21, the same code as tag. A null name and the name None share one translator. The empty string does not. The first load allocates, parses the original name, parses a zero token, then frees. A throw leaves the translator unloaded so the next call retries.

| Managed (`VmXlat1217Slice.cs`) | Native (`parse_xlat.cpp`) |
|---------------------------------|---------------------------|
| Keyword table and load | **1–167** |
| Band hull (exclusive slice) | **1–167** |

### Chunk **1217** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **167** |
| **Net headline** | **+167** vs post-**1216** |


**Post-review ratio:** 139/167 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `VmXlat1217SliceTests.FindToken_MatchesTheSortedKeywordsAndLeavesAMissUntouched`
- `VmXlat1217SliceTests.Load_ParsesOncePerNameAndKeepsNoneApartFromEmpty`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1217 --to 1217` (0 hard / 0 soft).

## Seams

stricmp is ASCII case-fold. lineid stores the tag code 21. A null lump name is NAME_None and shares the translator for the string None. The empty string is a different key. ParseLump receives the original lump name. A throw before loaded is set skips the free and retries.

## Credit rationale

UNCredited parse_xlat.cpp 1-167 (+167). The file ends at 167. A full tally line scan shows no overlap.
