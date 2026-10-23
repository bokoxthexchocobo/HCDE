# Chunk 847 — `FParser::SF_LevelNum` … `SF_SetColor` from `fragglescript/t_func.cpp` (3541–3698).

## Scope

`FParser::SF_LevelNum` … `SF_SetColor` from `fragglescript/t_func.cpp` (3541–3698).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_LevelNum** 3541–3545 (5 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MobjRadius** 3554–3568 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MobjHeight** 3577–3591 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ThingCount** 3600–3652 (53 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SetColor** 3660–3698 (39 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF level separators** 3546–3553 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF level separators** 3569–3576 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF level separators** 3592–3599 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF level separators** 3653–3659 (7 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmFScriptParserSfLevel.cs`
Managed LOC (tally): 155.
Conversion commit: `482bd6d030a9809548794af51815910cc105e8ec`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_LevelNum` | yes |
| `SF_MobjRadius` | yes |
| `SF_MobjHeight` | yes |
| `SF_ThingCount` | yes |
| `SF_SetColor` | yes |

| Managed | Native (`t_func.cpp` 3541–3698) |
|---------|-------------------------|
| `SfLevelNum` | **3541–3545** |
| `SfMobjRadius` | **3554–3568** |
| `SfMobjHeight` | **3577–3591** |
| `SfThingCount` | **3600–3652** |
| `SfSetColor` | **3660–3698** |
| (separators) | **3546–3553**, **3569–3576**, **3592–3599**, **3653–3659** |

**Post-review ratio:** 155/158 managed/native ≈ 98% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfLevelTests.SfLevelNum_ReadsMapLevelNumber`
- `VmFScriptParserSfLevelTests.SfMobjRadius_ReadWrite`
- `VmFScriptParserSfLevelTests.SfThingCount_CountsZombiemen`
- `VmFScriptParserSfLevelTests.SfSetColor_TaggedSector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 847 --to 847` (0 hard / 0 soft).

## Seams

Uses `AuthoritySimulation`, `DoomActorCatalog`, `AcsThingCountReplacementPolicy`, and `LevelSector.ColormapLightColor` for script sector tint.

## Credit rationale

Portable GZDoom query band after **846** trig; **1381–2241** remains deferred.
