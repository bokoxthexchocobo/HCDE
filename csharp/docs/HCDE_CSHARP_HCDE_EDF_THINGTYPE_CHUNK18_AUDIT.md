# hcde_edf.cpp thingtype parse subset — chunk 18 audit

## Top absent native files (headless-relevant, by remaining LOC)

Derived from `native_managed_matrix.json` engine scope minus credited `loc_tally.json` ranges (excludes rendering/UI/thirdparty). **Remaining** ≈ file lines − credited conversion lines.

| Remaining | File | Notes |
|----------:|------|--------|
| 13,469 | `src/common/scripting/backend/codegen.cpp` | ZScript backend codegen |
| 10,617 | `src/playsim/p_acs.cpp` | ACS VM bulk still native |
| 10,178 | `src/d_net.cpp` | Dedicated net driver |
| 9,061 | `src/playsim/p_mobj.cpp` | Actor mobj core |
| 7,809 | `src/playsim/p_actionfunctions.cpp` | Action functions |
| 7,287 | `src/playsim/p_map.cpp` | Movement/collision |
| 6,808 | `src/zcc-parse.c` | ZScript parser generated |
| 4,393 | `src/gamedata/d_dehacked.cpp` | Dehacked |
| 3,712 | `src/common/scripting/frontend/zcc_compile.cpp` | ZScript compile front-end |
| 3,589 | `src/maploader/maploader.cpp` | Map load |
| 3,151 | `src/gamedata/g_mapinfo.cpp` | MAPINFO |
| 1,999 | `src/xlat_parser.c` | Line-special translator parser |
| 1,604 | `src/gamedata/hcde_edf.cpp` | HCDE EDF subset (**this chunk**) |
| 1,367 | `src/scripting/decorate/thingdef_parse.cpp` | Decorate thingdef |

## Scope (this chunk)

Port `ParseThingHeader`, `ParseThingPropertyLine` (doomednum / dehackednum / spawnid / inherits / compatname), and flat property scan from `ParseThingBlock` in `hcde_edf.cpp`.

**Conversion native LOC credited:** 88 (`ParseThingPropertyLine` 663–727 subset, `ParseThingHeader` 729–756 subset, `ParseThingBlock` 927–980 subset).

**Managed LOC credited:** 108 (`HcdeEdfThingParser.cs`).

## Tests

- `HcdeEdfThingParserTests.ThingType_ParsesDoomednumAndInherits`
- `HcdeEdfThingParserTests.ThingDelta_MarksUnknownBehaviorProperty`
- `HcdeEdfThingParserTests.MissingName_ReturnsFalse`

## Verified

`dotnet build -warnaserror`; full `dotnet test`.
