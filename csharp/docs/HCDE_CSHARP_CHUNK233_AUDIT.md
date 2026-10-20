# Chunk 233 — ReadAllDecals and decallib scattered gap lines.

## Scope

ReadAllDecals and decallib scattered gap lines.

## Native ranges

- `src/gamedata/decallib.cpp` **FDecalLib ReadAllDecals** 248–287 (40 lines)
- `src/gamedata/decallib.cpp` **decallib gap** 330–330 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 2** 349–349 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 3** 484–484 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 4** 520–520 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 5** 562–562 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 6** 599–599 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 7** 650–650 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 8** 703–703 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 9** 747–747 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap block** 778–799 (22 lines)
- `src/gamedata/decallib.cpp` **decallib gap 10** 862–862 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 11** 869–869 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 12** 897–897 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 13** 934–934 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 14** 955–964 (10 lines)
- `src/gamedata/decallib.cpp` **decallib gap 15** 989–994 (6 lines)
- `src/gamedata/decallib.cpp` **decallib gap 16** 1037–1037 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 17** 1052–1052 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 18** 1070–1070 (1 lines)
- `src/gamedata/decallib.cpp` **decallib gap 19** 1172–1172 (1 lines)

**Credited native LOC:** 95 (tally `nativeLoc` 95).

## Managed

(see conversion commit file list)
Managed LOC (tally): 60.
Conversion commit: `20d15060ccd8058244d9e6fca30fb9834482d81c`.

## Tests

- `VmDecalDocumentLoaderTests.LoadAllDecalLumps_ParsesDecalBlock`
- `VmDecalLibCoreTests.Clear_ResetsRoot`
- `VmDecalKeywordSchemaTests.DecalKeywords_EndsWithTranslatable`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 233 --to 233` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
