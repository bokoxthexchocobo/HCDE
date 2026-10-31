# Chunk 1353 — Activator camera-restore tail and LS_SetPlayerProperty from p_lnspec.cpp (2869-3129).

## Scope

Activator camera-restore tail and LS_SetPlayerProperty from p_lnspec.cpp (2869-3129).

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_SetPlayerProperty** 2869–3129 (261 lines)

**Credited native LOC:** 261 (tally `nativeLoc` 261).

## Managed

`VmPlayerProperty1353Slice.cs`
Managed LOC (tally): 356.
Conversion commit: `d1f18a538e39cf47c40fa310e347db5103b17e60`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Activator camera restore (**2869–2880**) | yes |
| Property enum (**2882–2909**) | yes |
| Power give and take (**2911–3024**) | yes |
| Cheat flag masks and the frozen link (**3026–3128**) | yes |
| Band hull (**2869–3129**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| GiveInventoryType | inventory creation stays on the actor |
| FindActor | class lookup stays on the level |
| FindInventory | name and class lookup stay on the actor |
| R_ClearPastViewer | the viewer reset stays outside this band |
| Camera actor arm | lines through **2868** stay in chunk 931 |

The credited camera lines run only after the null-camera else. They store the activator, clear CF_REVERTPLEASE, and clear the past viewer only when the pointer changed, then return true. A missing activator or a missing player returns false when who is 0. Powers from invulnerability through speed give or take. Unused slots 8 and 9 return false. Allmap writes LEVEL2_ALLMAP only for the console player, and a missing body skips that player. Invulnerability with value 1 stores the inverse special colormap 0x00b60000. The activator take looks up the power name. The every-player take looks up the class. Fly leaves the cheat mask at 0, so the call returns false, and still sets MF7_FLYCHEAT, MF2_FLY, and MF_NOGRAVITY on the activator. Every-player fly does not touch MF7_FLYCHEAT. Noclip2 sets both noclip bits. With the frozen compat bit, clearing or giving one frozen cheat on every player writes both. The activator path does not link those bits.

| Managed (VmPlayerProperty1353Slice.cs) | Native (p_lnspec.cpp) |
|----------------------------------------|----------------------|
| Camera tail, powers, and cheat masks | **2869–3129** |
| Band hull (exclusive slice) | **2869–3129** |

### Chunk **1353** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **261** |
| **Net headline** | **+261** vs the prior chunk |


**Post-review ratio:** 356/261 managed/native ≈ 136% (src file LOC at HEAD; see tally).

## Tests

- `VmPlayerProperty1353SliceTests.CameraTailAndPowers`
- `VmPlayerProperty1353SliceTests.FlagMasksAndFrozenLink`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1353 --to 1353` (0 hard / 0 soft).

## Seams

GiveInventoryType, FindActor, FindInventory, and R_ClearPastViewer stay injected. The camera-actor arm of LS_ChangeCamera stays in chunk 931.

## Credit rationale

UNCredited p_lnspec.cpp 2869-3129 (+261). A full tally line scan shows no overlap. This is the largest remaining hole of at most 400 lines, ahead of gi.cpp 1-260.
