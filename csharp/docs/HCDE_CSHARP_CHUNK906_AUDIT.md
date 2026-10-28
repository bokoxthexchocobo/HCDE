# Chunk 906 — `FParser::SF_ObjX` … `SF_PushThing` from `fragglescript/t_func.cpp` (**919–1215**). Folds post-**905** separators **919–926**; stops before `SF_ReactionTime` (**1224**).

## Scope

`FParser::SF_ObjX` … `SF_PushThing` from `fragglescript/t_func.cpp` (**919–1215**). Folds post-**905** separators **919–926**; stops before `SF_ReactionTime` (**1224**).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjX** 919–1215 (297 lines)

**Credited native LOC:** 297 (tally `nativeLoc` 297).

## Managed

`VmFScriptParserSfObj.cs`, `VmFScriptParserSfObjState.cs`, `VmFScriptParserSfPushThing906Slice.cs`
Managed LOC (tally): 195.
Conversion commit: `337379b6c1236b43462963bfadcaef37850c416c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native `SF_*` | Behavior ported |
|---------------|-----------------|
| `SF_ObjX` / `Y` / `Z` / `ObjAngle` | yes |
| `SF_Teleport` / `SF_SilentTeleport` | yes |
| `SF_DamageObj` | yes |
| `SF_ObjSector` / `SF_ObjHealth` | yes |
| `SF_ObjFlag` | yes |
| `SF_PushThing` | yes |

| Managed | Native (`t_func.cpp`) |
|---------|----------------------|
| `SfObjX` | **927–941** |
| `SfObjY` | **949–963** |
| `SfObjZ` | **971–985** |
| `SfObjAngle` | **994–1008** |
| `SfTeleport` | **1018–1039** |
| `SfSilentTeleport` | **1047–1068** |
| `SfDamageObj` | **1076–1097** |
| `SfObjSector` | **1106–1121** |
| `SfObjHealth` | **1130–1145** |
| `SfObjFlag` | **1153–1195** |
| `SfPushThing` | **1204–1215** |
| (separator) | **919–926** |
| (separator) | **942–948** |
| (separator) | **964–970** |
| (separator) | **986–993** |
| (separator) | **1009–1017** |
| (separator) | **1040–1046** |
| (separator) | **1069–1075** |
| (separator) | **1098–1105** |
| (separator) | **1122–1129** |
| (separator) | **1146–1152** |
| (separator) | **1196–1203** |

### Chunk **906** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **297** |
| **Net headline** | **+297** vs post-**905** |


**Post-review ratio:** 195/297 managed/native ≈ 66% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfObjTests.SfObjX_Trigger_ReturnsCoord`
- `VmFScriptParserSfObjTests.SfTeleport_OneArg_UsesTrigger`
- `VmFScriptParserSfObjTests.SfPushThing_ThrustsActor`
- `VmFScriptParserSfObjStateTests.SfObjHealth_Trigger_ReturnsHealth`
- `VmFScriptParserSfObjStateTests.SfDamageObj_Trigger_PassesDamage`
- `VmFScriptParserSfObjStateTests.SfObjFlag_ReadSetBit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 906 --to 906` (0 hard / 0 soft).

## Seams

Coords, `EV_Teleport`, damage, sector tag, flags, and `Thrust` are host/`Actor` callbacks; `Level->EV_Teleport` / `P_DamageMobj` not hosted.

## Credit rationale

Largest honest UNCredited slice from **927** through **1215** (**297** lines); `SfPushThing` in exclusive **906** slice so `VmFScriptParserSfMom.cs` stays free for **1224+**.
