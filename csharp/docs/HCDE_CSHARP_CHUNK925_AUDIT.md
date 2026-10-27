# Chunk 925 — `DLevelScript::CallFunction` tail (**6916–7007**), `PRINTNAME` / enhanced PC readers, `ACS_RunConsoleCommandString`, and `CharArrayParms` head from `p_acs.cpp` (**6916–7065**). Continues **924**; `RunScript` at **7092** deferred.

## Scope

`DLevelScript::CallFunction` tail (**6916–7007**), `PRINTNAME` / enhanced PC readers, `ACS_RunConsoleCommandString`, and `CharArrayParms` head from `p_acs.cpp` (**6916–7065**). Continues **924**; `RunScript` at **7092** deferred.

## Native ranges

- `src/playsim/p_acs.cpp` **DLevelScript::CallFunction** 6916–7065 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPAcsCallfunc925Slice.cs`
Managed LOC (tally): 125.
Conversion commit: `0c4e071cd13a130f9fcabd5c41ff2e67bc5454e1`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `GetLineHealth` tail (**6916–6918**) | yes |
| `GetLineX`/`Y` (**6919–6934**) | yes |
| `SetSubtitleNumber` / invasion ACSF (**6936–7001**) | yes |
| `CallFunction` close (**7003–7007**) | yes |
| `PRINTNAME_*` (**7010–7017**) | yes |
| `NEXTWORD`…`TAGSTR` manifest (**7020–7026**) | yes |
| `getbyte` / `getshort` (**7028–7040**) | yes |
| `ACS_RunConsoleCommandString` (**7042–7061**) | yes |
| `CharArrayParms` head (**7063–7065**) | yes |

| Managed (`VmPAcsCallfunc925Slice.cs`) | Native (`p_acs.cpp`) |
|--------------------------------------|---------------------|
| `TryGetLineCoordinate` (incl. health tail seam) | **6916–6934** |
| `TrySetSubtitleNumber` | **6936–6952** |
| `TryInvasionCall` | **6954–7001** |
| `CallFunction` fall-through | **7003–7007** |
| `PrintName*` constants | **7010–7017** |
| Reader manifest (`NEXTBYTE`/`NEXTSHORT` parity) | **7020–7026** |
| `ReadEnhancedByte` / `ReadEnhancedShort` | **7028–7040** |
| `RunScriptConsoleCommand` | **7042–7061** |
| `TryPopCharArrayParms` | **7063–7065** |

### Chunk **925** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**924** |


**Post-review ratio:** 125/150 managed/native ≈ 83% (src file LOC at HEAD; see tally).

## Tests

- `AcsCallFunctionTests.CallFunc_GetLineX_ReturnsMidpointAlongTaggedLine`
- `AcsCallFunctionTests.CallFunc_GetLineY_AppliesPerpendicularOffset`
- `AcsAbsentCallFuncChunk11Tests.CallFunc_StartSlideshowAndSubtitle_StorePresentationState`
- `AcsAbsentOpcodeChunk10Tests.CallFunc_InvasionGetWave_Wire19702`
- `AcsCallFunctionTests.CallFunc_GetLineHealth_ReturnsFirstTaggedLineHealth`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 925 --to 925` (0 hard / 0 soft).

## Seams

`Net_*` invasion exports, `AddCommandString`, and full `FACSStackMemory` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band in **6614–7509** corridor; exclusive `VmPAcsCallfunc925Slice.cs`.
