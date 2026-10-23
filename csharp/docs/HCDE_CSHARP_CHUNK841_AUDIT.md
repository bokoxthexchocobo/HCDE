# Chunk 841 — `FParser::SF_SilentTeleport` … `SF_ObjFlag` from `fragglescript/t_func.cpp` (1041–1200).

## Scope

`FParser::SF_SilentTeleport` … `SF_ObjFlag` from `fragglescript/t_func.cpp` (1041–1200).

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_SilentTeleport** 1047–1068 (22 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_DamageObj** 1076–1097 (22 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjSector** 1106–1121 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjHealth** 1130–1145 (16 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_ObjFlag** 1153–1195 (43 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF obj-state separators** 1041–1046 (6 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF obj-state separators** 1069–1075 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF obj-state separators** 1098–1105 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF obj-state separators** 1122–1129 (8 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF obj-state separators** 1146–1152 (7 lines)
- `src/playsim/fragglescript/t_func.cpp` **FParser SF obj-state separators** 1196–1200 (5 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Portfolio delta

**Gross CLI band** (`1041–1200`): **160** lines in the UNCredited-range probe; tally credits **160** whole-function native LOC only.

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **160** |
| **Credited** (whole functions, this row) | **160** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+160** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptParserSfObjState.cs`
Managed LOC (tally): 104.
Conversion commit: `515fcd4c23210f5a0d42dcb299002e5923782ecc`.

## Padding review (2026-10-21)

| Managed | Native (`t_func.cpp`) |
|---------|-------------------------|
| `SfSilentTeleport` | **1047–1068** |
| `EvSilentTeleport` | **1065–1066** |
| `SfDamageObj` | **1076–1097** |
| `DamageActor` | **1094–1095** |
| `SfObjSector` | **1106–1121** |
| `SectorFirstTag` | **1120** |
| `OptionalActor` | **1109–1117**, **1133–1141** |
| `SfObjHealth` | **1130–1145** |
| `ActorHealth` | **1144** |
| `SfObjFlag` | **1153–1195** |
| `GetActorFlags` | **1178–1191** |
| `SetActorFlags` | **1185** |
| (native separators) | **1041–1046**, **1069–1075**, **1098–1105**, **1122–1129**, **1146–1152**, **1196–1200** |

**Post-review ratio:** 104/160 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserSfObjStateTests.SfDamageObj_Trigger_PassesDamage`
- `VmFScriptParserSfObjStateTests.SfObjHealth_Trigger_ReturnsHealth`
- `VmFScriptParserSfObjStateTests.SfObjFlag_ReadSetBit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 841 --to 841` (0 hard / 0 soft).

## Seams

Teleport, damage, sector tag, and actor flags are host callbacks on `ParserContext` (`VmFScriptParserSfObjState.cs` only).

## Credit rationale

Whole-function band after **`SF_Teleport` (**840**)** through **`SF_ObjFlag`** before **`SF_PushThing` (**842**).
