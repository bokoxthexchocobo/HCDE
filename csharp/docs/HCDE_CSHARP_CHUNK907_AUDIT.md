# Chunk 907 — `FParser::SF_ReactionTime` … `SF_PointToDist` from `fragglescript/t_func.cpp` (**1216–1378**). Folds post-**906** separators **1216–1223**; stops before `SF_SetCamera` (**1391**).

## Scope

`FParser::SF_ReactionTime` … `SF_PointToDist` from `fragglescript/t_func.cpp` (**1216–1378**). Folds post-**906** separators **1216–1223**; stops before `SF_SetCamera` (**1391**).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ReactionTime** 1216–1378 (163 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmFScriptParserSfMom.cs`
Managed LOC (tally): 106.
Conversion commit: `02951d0b6d97b2d9caa514fb2deb11223222dac6`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_ReactionTime` | yes |
| `SF_MobjTarget` | yes |
| `SF_MobjMomx` / `Momy` / `Momz` | yes |
| `SF_PointToAngle` / `SF_PointToDist` | yes |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
| `SfReactionTime` | **1224–1238** |
| `SfMobjTarget` | **1247–1269** |
| `SfMobjMomx` | **1277–1292** |
| `SfMobjMomy` | **1300–1315** |
| `SfMobjMomz` | **1323–1338** |
| `SfPointToAngle` | **1349–1360** |
| `SfPointToDist` | **1369–1378** |
| (separator) | **1216–1223** |
| (separator) | **1239–1246** |
| (separator) | **1270–1276** |
| (separator) | **1293–1299** |
| (separator) | **1316–1322** |
| (separator) | **1339–1346** |
| (separator) | **1361–1368** |

### Chunk **907** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **163** |
| **Net headline** | **+163** vs post-**906** |


**Post-review ratio:** 106/163 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfMomTests.SfReactionTime_ReadWrite`
- `VmFScriptParserSfMomTests.SfMobjTarget_SetsTarget`
- `VmFScriptParserSfMomTests.SfMobjMomx_ReadWrite`
- `VmFScriptParserSfMomTests.SfMobjMomy_ReadWrite`
- `VmFScriptParserSfMomTests.SfMobjMomz_ReadWrite`
- `VmFScriptParserSfMomTests.SfPointToAngle_East`
- `VmFScriptParserSfMomTests.SfPointToDist_AxisDistance`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 907 --to 907` (0 hard / 0 soft).

## Seams

`SetState`, `AcsActorPointer`, and `Actor` velocity/target fields are playsim-hosted; camera natives deferred.

## Credit rationale

Honest **163**-line UNCredited band on exclusive `VmFScriptParserSfMom.cs` after **906** `PushThing` split; function-bounded through `PointToDist`.
