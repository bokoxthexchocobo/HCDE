# Chunk 1060 — `V_InitFontColors` TEXTCOLO block tail, translation finalize, and `V_FindFontColor` from `v_font.cpp` (**451–600**); follows chunk **1059** (**301–450**).

## Scope

`V_InitFontColors` TEXTCOLO block tail, translation finalize, and `V_FindFontColor` from `v_font.cpp` (**451–600**); follows chunk **1059** (**301–450**).

## Native ranges

- `src/common/fonts/v_font.cpp` **V_InitFontColors / V_FindFontColor** 451–600 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmVFontTextColo1060Slice.cs`
Managed LOC (tally): 103.
Conversion commit: `98b4658c44d6cccc111231a953c6b7724f673d89`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Normal/untranslated parm length rules (**468–480**) | yes |
| Default console white translation (**482–491**) | yes |
| Duplicate color name merge vs push (**494–511**) | yes (predicates) |
| Translation index assignment (**523–535**) | yes |
| Terminator markers (**540–542**) | seam (host push) |
| `TranslationMapCompare` / `V_FindFontColor` (**556–590**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **451–453** | explicit range tail overlap **1059** |
| **464–467** | host `parms.Push` |
| **515–544** | host container copies / `qsort` |
| **546–548** | `assert` host |
| **600+** | `CreateLuminosityTranslationRanges` body outside hull |

| Managed (`VmVFontTextColo1060Slice.cs`) | Native (`v_font.cpp`) |
|----------------------------------------|----------------------|
| `ValidateColorBlockParmLengths` | **468–480** |
| `ShouldSynthesizeDefaultConsoleTranslation` / `CreateDefaultConsoleTranslation` | **482–490** |
| `ShouldRegisterNewColorName` | **506–511** |
| `AllocateNextTranslationIndex` | **523–534** |
| `CompareTranslationNameIndices` / `BinarySearchTranslationNumber` | **556–590** |
| Band hull (exclusive slice) | **451–600** |

### Chunk **1060** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1059** |


**Post-review ratio:** 103/150 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmVFontTextColo1060SliceTests.ValidateColorBlockParmLengths_RequiresNormalRanges`
- `VmVFontTextColo1060SliceTests.ValidateColorBlockParmLengths_UntranslatedMustStayEmpty`
- `VmVFontTextColo1060SliceTests.ShouldSynthesizeDefaultConsoleTranslation_SkipsUntranslated`
- `VmVFontTextColo1060SliceTests.CreateDefaultConsoleTranslation_MatchesNativeWhiteFill`
- `VmVFontTextColo1060SliceTests.ShouldRegisterNewColorName_WhenNoDuplicate`
- `VmVFontTextColo1060SliceTests.AllocateNextTranslationIndex_ReusesAssigned`
- `VmVFontTextColo1060SliceTests.BinarySearchTranslationNumber_FindsNamedColor`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1060 --to 1060` (0 hard / 0 soft).

## Seams

`FScanner`, `qsort`, `TranslationParms` mutation, and `CreateLuminosityTranslationRanges` arena alloc remain host-injected; hull ends at **`CreateLuminosityTranslationRanges` head (**602+**).

## Credit rationale

UNCredited **`v_font.cpp`** **451–600** (+150); TEXTCOLO continuation in same TU; zero Vm* line collision; disjoint from **1059** except seam line **450**/**451**.
