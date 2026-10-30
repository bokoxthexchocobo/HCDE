# Chunk 1249 — Model table, skin lookup, frame hash, and format dispatch from model.cpp (1-229), through end of file.

## Scope

Model table, skin lookup, frame hash, and format dispatch from model.cpp (1-229), through end of file.

## Native ranges

- `src/common/models/model.cpp` **FlushModels** 1–229 (229 lines)

**Credited native LOC:** 229 (tally `nativeLoc` 229).

## Managed

`VmModel1249Slice.cs`
Managed LOC (tally): 200.
Conversion commit: `65af74ae27ad37984a5fc7a7ee75e9feb65852ff`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| FlushModels and vertex buffers (**43–71**) | yes |
| FindGFXFile (**79–97**) | yes |
| LoadSkin (**106–115**) | yes |
| ModelFrameHash (**123–126**) | yes |
| FindModel (**135–229**) | yes |
| Band hull (**1–229**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Model class bodies | other model files |
| Voxel decoder | voxel loader |

An exact skin name is kept. Otherwise the extension is removed when the dot sits after the slash, and the highest lump among png, jpg, tga, and pcx wins. FindModel rejects a missing file and a file of 2147483648 bytes or more. A cached name matches without case. Magic DMDM, IDP2, IDP3, and INTERQUAKEMODEL select those models. Anything else tries a voxel. Flush clears the three renderer slots from the last model backward and leaves the list in place.

| Managed (`VmModel1249Slice.cs`) | Native (`model.cpp`) |
|----------------------------------|----------------------|
| Skin, hash, and format dispatch | **1–229** |
| Band hull (exclusive slice) | **1–229** |

### Chunk **1249** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **229** |
| **Net headline** | **+229** vs post-**1248** |


**Post-review ratio:** 200/229 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmModel1249SliceTests.Find_DispatchesMagicAndKeepsTheHigherSkinLump`
- `VmModel1249SliceTests.Find_RejectsAMissingFileAndAFailedLoad`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1249 --to 1249` (0 hard / 0 soft).

## Seams

Lump lookup, file bytes, texture ids, model Load, and the voxel reader are host tables. A companion _a.3d or _d.3d lump must be greater than 0 before an Unreal model is chosen. silent is accepted and does not change the messages. The native 'File to large' wording is kept.

## Credit rationale

UNCredited model.cpp 1-229 (+229). A full tally line scan shows no overlap. The file ends at line 229.
