# Chunk 1183 — FraggleScript section registration, serialize, and hash lookup from t_prepro.cpp (1–212).

## Scope

FraggleScript section registration, serialize, and hash lookup from t_prepro.cpp (1–212).

## Native ranges

- `src/playsim/fragglescript/t_prepro.cpp` **DFsScript::FindSectionEnd** 1–212 (212 lines)

**Credited native LOC:** 212 (tally `nativeLoc` 212).

## Managed

`VmTPreproSections1183Slice.cs`
Managed LOC (tally): 128.
Conversion commit: `6f772d98f1de4c02c321d3b0b5fbd0fe14aeda42`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| DFsSection registration (**53–57**) | yes |
| Serialize (**65–73**) | yes |
| SectionStart (**81–84**) | yes |
| SectionEnd (**92–95**) | yes |
| SectionLoop (**103–106**) | yes |
| ClearSections (**114–127**) | yes |
| NewSection (**135–145**) | yes |
| FindSectionStart (**153–167**) | yes |
| FindSectionEnd (**176–197**) | yes |
| Band hull (**1–212**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **213–425** | ProcessFindChar, dry run, preprocess, and include stay in the next band |

The class is not abstract and publishes the next pointer. Serialize runs the base serializer first, then copies type, start, end, loop, and next. Start, end, and loop are the stored indexes. Clear walks all 17 hash slots, saves the link, destroys the section, then nulls the slot. NewSection hashes the brace index by 17, pushes the section on that chain, stores the start index, and records a write barrier. Find start searches only that chain. Find end searches every chain because the hash is the opening brace.

| Managed (`VmTPreproSections1183Slice.cs`) | Native (`t_prepro.cpp`) |
|--------------------------------------------|------------------------|
| Section table | **1–212** |
| Band hull (exclusive slice) | **1–212** |

### Chunk **1183** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **212** |
| **Net headline** | **+212** vs post-**1182** |


**Post-review ratio:** 128/212 managed/native ≈ 60% (src file LOC at HEAD; see tally).

## Tests

- `VmTPreproSections1183SliceTests.RegistrationAndSerialize_RecordThePointerAndRunSuperFirst`
- `VmTPreproSections1183SliceTests.NewSection_PushesTheFrontOfTheHashChain`
- `VmTPreproSections1183SliceTests.ClearSections_DestroysTheSavedLinkBeforeNullingTheSlot`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1183 --to 1183` (0 hard / 0 soft).

## Seams

Serialize calls the base serializer before copying fields. NewSection records the GC write barrier. Destroy clears the saved link only after the walk has copied it. ProcessFindChar at 213 stays outside this slice.

## Credit rationale

UNCredited t_prepro.cpp 1–212 (+212). FindSectionEnd closes at 197. The ProcessFindChar banner is 199–212 and the function starts at 213. The whole file is 425 lines, over the 400 ceiling, so this band stops on that boundary. Chunk 857 retired the later preprocessor with empty native ranges. A full tally line scan shows no overlap.
