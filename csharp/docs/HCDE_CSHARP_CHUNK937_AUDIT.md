# Chunk 937 — `SF_SetWeapon` close, `SF_MoveCamera`, `SF_ObjAwaken` … `SF_MobjValue` open from `fragglescript/t_func.cpp` (**2685–2834**). Pivot from **p_lnspec** tail; continues **2685–3244** corridor.

## Scope

`SF_SetWeapon` close, `SF_MoveCamera`, `SF_ObjAwaken` … `SF_MobjValue` open from `fragglescript/t_func.cpp` (**2685–2834**). Pivot from **p_lnspec** tail; continues **2685–3244** corridor.

## Native ranges

- `src/playsim/fragglescript/t_func.cpp` **FParser::SF_MoveCamera** 2685–2834 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmFScriptParserMoveCamera937Slice.cs`
Managed LOC (tally): 152.
Conversion commit: `3e2cd68e0fabbbfea45abdf8296acbc015b20360`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`fragglescript/t_func.cpp`) |
|---------|-----------------------------------|
| `SetWeaponRequiresTwoArgs` | `SF_SetWeapon` **2679–2683** |
| `BuildMoveCamera` / `MoveToward` | `SF_MoveCamera` **2694–2765** |
| `StepCameraYaw` / `DeltaAngleDegrees` | angle path **2728–2761** |
| `MoveCameraReturn` | return **2763–2764** |
| `CameraColliderScale` | radius/height **2715–2716** |
| `AwakenActor` | `SF_ObjAwaken` **2777–2794** |
| `AmbientSoundRequiresArgs` | `SF_AmbientSound` **2805–2810** |
| `MarkSecretExit` | `SF_ExitSecret` **2822–2825** |
| `MobjValueRequiresArgs` | `SF_MobjValue` **2834–2840** |
| `MoveCameraRejectInvalidActors` | guard **2700–2703** |
| `MoveFinished` | move complete **2721–2726** |
| chunk **937** hull | **2685–2834** |

### Chunk **937** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**936** |


**Post-review ratio:** 152/150 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptParserMoveCamera937SliceTests.MoveToward_ClampsToSpeed`
- `VmFScriptParserMoveCamera937SliceTests.BuildMoveCamera_RejectsNullTarget`
- `VmFScriptParserMoveCamera937SliceTests.BuildMoveCamera_ReturnsZeroWhenFinished`
- `VmFScriptParserMoveCamera937SliceTests.AwakenActor_UsesTriggerWhenNoArgs`
- `VmFScriptParserMoveCamera937SliceTests.DeltaAngleDegrees_WrapsAcrossZero`
- `VmFScriptParserMoveCamera937SliceTests.SetWeaponRequiresTwoArgs`
- `VmFScriptParserMoveCamera937SliceTests.MobjValueRequiresArgs`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 937 --to 937` (0 hard / 0 soft).

## Seams

`ScriptUtil::Exec`, `S_Sound`, `actorvalue`/`floatvalue`, and `CallActivate` remain host seams.

## Credit rationale

Honest **150**-line UNCredited band; exclusive `VmFScriptParserMoveCamera937Slice.cs` (table-slot / prior `SfAct` bodies not matrix-credited).
