# Chunk 936 — `LineSpecials[]` opcodes **208–284** and `LineSpecialTable` / `P_GetMaxLineSpecial` head (`p_lnspec.cpp` **3755–3904**). Follows **935** (through **207** at **≤3754**); **3905–3982** sub-150 deferred.

## Scope

`LineSpecials[]` opcodes **208–284** and `LineSpecialTable` / `P_GetMaxLineSpecial` head (`p_lnspec.cpp` **3755–3904**). Follows **935** (through **207** at **≤3754**); **3905–3982** sub-150 deferred.

## Native ranges

- `src/playsim/p_lnspec.cpp` **LineSpecials** 3755–3904 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecLineSpecialDispatch936Slice.cs`
Managed LOC (tally): 130.
Conversion commit: `2b2579b17f1b45d04e95e73806f2d45637f2877c`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_lnspec.cpp`) |
|---------|-------------------------|
| chunk **936** hull (dispatch tail + registry head) | **3755–3904** |
| `DispatchOpcodes208Through284` | `LineSpecials[]` **3755–3833** |
| `TailDispatchName` / `TailDispatchIsNop` | opcode lookup **3755–3832** |
| `TailDispatchIsSectorEnv` | **3759–3767** |
| `TailDispatchIsElevator` | **3792–3794** |
| `TailDispatchIsExit` | **3790–3791** |
| `LS_ChangeCamera` table slot | **3786** (handler elsewhere) |
| `DEFINE_SPECIAL` / `LineSpecialNames` | **3835–3838** |
| `lscmp` name compare seam | **3840–3843** |
| `ScanMaxSpecialNumber` | ctor max scan **3851–3856** |
| `RegistrySlotCount` | `Resize(max+1)` **3857** |
| null slot init loop | **3858–3861** |
| `qsort` populate loop | **3863–3868** |
| `CanAssignRegistrySlot` | slot index bounds **3864–3867** |
| `MaxLineSpecialFromRegistrySize` | `P_GetMaxLineSpecial` **3878–3881** |
| `LineSpecialInfoInRange` | `P_GetLineSpecialInfo` **3889–3896** |
| `P_FindLineSpecial` header only | comment **3897–3903** |
| glow/automap slots | **3826–3831** |
| floor/ceiling EE cluster | **3804–3825** |
| ACS always / scroll cluster | **3770–3778** |
| transfer NOP slots | **3756–3758**, **3769**, **3774** |
| plat/light tail | **3779–3785** |
| stairs/door stop cluster | **3818–3825** |

### Chunk **936** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**935** |


**Post-review ratio:** 130/150 managed/native ≈ 87% (src file LOC at HEAD; see tally).

## Tests

- `VmPLnspecLineSpecialDispatch936SliceTests.TailDispatchEntryCount_Is77`
- `VmPLnspecLineSpecialDispatch936SliceTests.TailDispatchName_StartsAtTranslucentLine`
- `VmPLnspecLineSpecialDispatch936SliceTests.TailDispatchName_ChangeCameraIsTableSlotOnly`
- `VmPLnspecLineSpecialDispatch936SliceTests.TailDispatchName_EndsAtGenericCrusherDist`
- `VmPLnspecLineSpecialDispatch936SliceTests.ScanMaxSpecialNumber_FindsLargest`
- `VmPLnspecLineSpecialDispatch936SliceTests.MaxLineSpecialFromRegistrySize_MatchesNative`
- `VmPLnspecLineSpecialDispatch936SliceTests.CanAssignRegistrySlot_RejectsOutOfRange`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 936 --to 936` (0 hard / 0 soft).

## Seams

`actionspecials.h`, `qsort`/`stricmp`, and `P_FindLineSpecial` search body remain host seams.

## Credit rationale

Honest **150**-line UNCredited band completing dispatch tail plus registry sizing; exclusive `VmPLnspecLineSpecialDispatch936Slice.cs`.
