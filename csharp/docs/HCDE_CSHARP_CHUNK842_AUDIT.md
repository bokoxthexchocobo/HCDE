# Chunk 842 — `FParser::SF_PushThing` … `SF_PointToDist` from `fragglescript/t_func.cpp` (1201–1378).

## Scope

`FParser::SF_PushThing` … `SF_PointToDist` from `fragglescript/t_func.cpp` (1201–1378).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PushThing** 1204–1215 (12 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ReactionTime** 1224–1238 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MobjTarget** 1247–1269 (23 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MobjMomx** 1277–1292 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MobjMomy** 1300–1315 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MobjMomz** 1323–1338 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PointToAngle** 1349–1360 (12 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_PointToDist** 1369–1378 (10 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 1201–1203 (3 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 1216–1223 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 1239–1246 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 1270–1276 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 1293–1299 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 1316–1322 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 1339–1348 (10 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF band separators** 1361–1368 (8 lines)

**Credited native LOC:** 178 (tally `nativeLoc` 178).

## Managed

`VmFScriptParserSfMom.cs`
Managed LOC (tally): 118.
Conversion commit: `a0d6010ef07933a7ad5d14bd9ac69999aee05872`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_PushThing` | yes (`Actor.Thrust`) |
| `SF_ReactionTime` | yes (`Actor.ReactionTime`) |
| `SF_MobjTarget` | yes (`SetTargetPointer`, `SeeState`, `JustHit`) |
| `SF_MobjMomx` | yes (`VelocityX`) |
| `SF_MobjMomy` | yes (`VelocityY`) |
| `SF_MobjMomz` | yes (`VelocityZ`) |
| `SF_PointToAngle` | yes |
| `SF_PointToDist` | yes |

| Managed | Native (`t_func.cpp` 1201–1378) |
|---------|-------------------------|
| `SfPushThing` | **1204–1215** |
| `SfReactionTime` | **1224–1238** |
| `SfMobjTarget` | **1247–1269** |
| `SfMobjMomx` | **1277–1292** |
| `SfMobjMomy` | **1300–1315** |
| `SfMobjMomz` | **1323–1338** |
| `SfPointToAngle` | **1349–1360** |
| `SfPointToDist` | **1369–1378** |
| (separators) | **1201–1203**, **1216–1223**, **1239–1246**, **1270–1276**, **1293–1299**, **1316–1322**, **1339–1348**, **1361–1368** |

**Post-review ratio:** 118/178 managed/native ≈ 66% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfMomTests.SfPushThing_ThrustsActor`
- `VmFScriptParserSfMomTests.SfReactionTime_ReadWrite`
- `VmFScriptParserSfMomTests.SfMobjTarget_SetsTarget`
- `VmFScriptParserSfMomTests.SfMobjMomx_ReadWrite`
- `VmFScriptParserSfMomTests.SfMobjMomy_ReadWrite`
- `VmFScriptParserSfMomTests.SfMobjMomz_ReadWrite`
- `VmFScriptParserSfMomTests.SfPointToAngle_East`
- `VmFScriptParserSfMomTests.SfPointToDist_AxisDistance`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 842 --to 842` (0 hard / 0 soft).

## Seams

No new `ParserContext` Action/Func forwarders. FraggleScript `actorvalue` mobjs map to managed `Actor` (`Thrust`, `ReactionTime`, `VelocityX/Y/Z`, `SetTargetPointer`/`SeeState`/`JustHit`, `Simulation` for target read). In-band trig: `SfPointToAngle` / `SfPointToDist`.

## Credit rationale

Whole-function band after **`SF_ObjFlag` (**841**)** through **`SF_PointToDist`** before **`SF_SetCamera` (**843+**).
