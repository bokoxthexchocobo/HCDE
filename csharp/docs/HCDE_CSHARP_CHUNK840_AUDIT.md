# Chunk 840 — `FParser::SF_RemoveObj` … `SF_Teleport` from `fragglescript/t_func.cpp` (877–1039).

## Scope

`FParser::SF_RemoveObj` … `SF_Teleport` from `fragglescript/t_func.cpp` (877–1039).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_RemoveObj** 877–888 (12 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_KillObj** 896–918 (23 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjX** 927–941 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjY** 949–963 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjZ** 971–985 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjAngle** 994–1008 (15 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_Teleport** 1018–1039 (22 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF mobj separators** 889–895 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF mobj separators** 919–926 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF mobj separators** 942–948 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF mobj separators** 964–970 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF mobj separators** 986–993 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF mobj separators** 1009–1017 (9 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmFScriptParserSfObj.cs`
Managed LOC (tally): 99.
Conversion commit: `0dd7cebc5f996384f7cbf108b0fd7104abe76f5d`.

## Padding review (2026-10-21)

| Managed | Native (`t_func.cpp`) |
|---------|-------------------------|
| `SvtFixed` | **113–117** (`setDouble` / `svt_fixed` in `t_script.h`) |
| `FixedValue` | **113–117** |
| `SetReturnDouble` | **940**, **962**, **984**, **1007** |
| `OptionalActor` | **927–941**, **949–963**, **971–985**, **994–1008** |
| `SfRemoveObj` | **877–888** |
| `ClearActorCounters` | **884** |
| `DestroyActor` | **885** |
| `SfKillObj` | **896–918** |
| `MarkActorMassacreKillable` | **912–914** |
| `DamageActorMassacre` | **916** |
| `ActorHealth` | **916** |
| `SfObjX` | **927–941** |
| `ActorX` | **940** |
| `SfObjY` | **949–963** |
| `ActorY` | **962** |
| `SfObjZ` | **971–985** |
| `ActorZ` | **984** |
| `SfObjAngle` | **994–1008** |
| `ActorYawDegrees` | **1007** |
| `SfTeleport` | **1018–1039** |
| `EvTeleportFog` | **1037** |
| (native separators) | **889–895**, **919–926**, **942–948**, **964–970**, **986–993**, **1009–1017** |

**Post-review ratio:** 99/163 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfObjTests.SfRemoveObj_CallsDestroy`
- `VmFScriptParserSfObjTests.SfObjX_Trigger_ReturnsCoord`
- `VmFScriptParserSfObjTests.SfTeleport_OneArg_UsesTrigger`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 840 --to 840` (0 hard / 0 soft).

## Seams

Mobj destroy, massacre damage, coords, and `EV_Teleport` are host callbacks on `ParserContext` (`VmFScriptParserSfObj.cs`); `ScriptValue.FixedValue` holds `svt_fixed` returns.

## Credit rationale

Whole-function band after spawn API (**839**) through `SF_Teleport` before `SF_SilentTeleport` (**841**).
