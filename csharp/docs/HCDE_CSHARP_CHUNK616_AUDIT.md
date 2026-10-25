# Chunk 616 — Net_DoCommand gaps: bots/centerview, summon/spray/mdk, autosave/FOV/scripts (8743–8754, 8800–8901, 8941–9008) Merged: [603].

## Scope

Net_DoCommand gaps: bots/centerview, summon/spray/mdk, autosave/FOV/scripts (8743–8754, 8800–8901, 8941–9008) Merged: [603].

## Native ranges

- `src/d_net.cpp` **Net_DoCommand** 8743–8745 (3 lines)
- `src/d_net.cpp` **Net_DoCommand** 8747–8750 (4 lines)
- `src/d_net.cpp` **Net_DoCommand** 8752–8754 (3 lines)
- `src/d_net.cpp` **Net_DoCommand** 8801–8888 (88 lines)
- `src/d_net.cpp` **Net_DoCommand** 8892–8895 (4 lines)
- `src/d_net.cpp` **Net_DoCommand** 8897–8900 (4 lines)
- `src/d_net.cpp` **Net_DoCommand** 8941–8950 (10 lines)
- `src/d_net.cpp` **Net_DoCommand** 8952–8954 (3 lines)
- `src/d_net.cpp` **Net_DoCommand** 8956–8968 (13 lines)
- `src/d_net.cpp` **Net_DoCommand** 8971–8973 (3 lines)
- `src/d_net.cpp` **Net_DoCommand** 8975–8982 (8 lines)
- `src/d_net.cpp` **Net_DoCommand** 8984–8990 (7 lines)
- `src/d_net.cpp` **Net_DoCommand** 8992–9008 (17 lines)

**Credited native LOC:** 167 (tally `nativeLoc` 167).

## Managed

`VmDNetDoCommandBotCenter.cs`, `VmDNetDoCommandSummon.cs`
Managed LOC (tally): 232.
Conversion commit: `3275413bb23ff4b073771b4b5a0d3e92062684ee`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `ApplyAddBotNative` | `DEM_ADDBOT` 8743–8745 |
| `ApplyKillBotsNative` | `DEM_KILLBOTS` 8747–8750 |
| `ApplyCenterViewNative` | `DEM_CENTERVIEW` 8752–8754 |
| `ApplySummonDoCommandNative` | `DEM_SUMMON*` 8801–8888 |
| `ApplySprayDecalNative` | `DEM_SPRAY` 8892–8895 |
| `ApplyMdkNative` | `DEM_MDK` 8897–8900 |
| `ApplyCheckAutosaveNative` | `DEM_CHECKAUTOSAVE` 8941–8950 |
| `ApplyDoAutosaveNative` | `DEM_DOAUTOSAVE` 8952–8954 |
| `ApplyFovDoCommandNative` | `DEM_FOV` 8956–8968 |
| `ApplyMyFovNative` | `DEM_MYFOV` 8971–8973 |
| `ApplyRunScriptNative` | `DEM_RUNSCRIPT`/`DEM_RUNSCRIPT2` 8975–8982 |
| `ApplyRunNamedScriptNative` | `DEM_RUNNAMEDSCRIPT` 8984–8990 |
| `ApplyRunSpecialNative` | `DEM_RUNSPECIAL` 8992–9008 |

<!-- merged from chunk603 -->
| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `Summon2ReadsExtendedFieldsNative` | 8815–8821 |
| `ShouldSpawnPlayerMissileNative` | 8829–8832 |
| `ShouldApplySummonFriendFlagsNative` | 8842–8854 |
| `ShouldClearSummonFriendFlagsNative` | 8855–8860 |
| `VisualThinkerClassAllowedNative` | 8877–8878 |
| `ShouldRunSprayOnLevelNative` | `DEM_SPRAY` 8892–8894 |
| `ShouldRunMdkCheatNative` | `DEM_MDK` 8897–8899 |
| `ShouldOfferAutosaveNative` | 8945–8947 |
| `FovChangedNative` | `DesiredFOV` compare 8959 |
| `RunScriptUsesAlwaysFlagNative` | `DEM_RUNSCRIPT2` 8980 |
| `ShouldRunNamedScriptNative` | 8984–8990 |
| `ShouldClampRunSpecialArgNative` | arg loop 9001–9002 |
| `ShouldExecuteRunSpecialNative` | `CheckCheatmode` 9005 |

### Out-of-body uncredit (616)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **167** |
| **Out-of-body removed** | **0** |
| **Honest** | **167** |


**Post-review ratio:** 232/167 managed/native ≈ 139% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetDoCommandBotCenterTests.ApplyCenterViewNative_SetsCentering`
- `VmDNetDoCommandBotCenterTests.ApplyKillBotsNative_RemovesAndPrints`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 616 --to 616` (0 hard / 0 soft).

## Seams

`BotInfo`, `Spawn`, `SprayDecal`, `cht_DoMDK`, `RunScript`, `P_ExecuteSpecial`, and `G_TraceSetGameAction` are invoked via host callbacks.

## Credit rationale

Whole `case` bodies for credited opcode ranges only.
