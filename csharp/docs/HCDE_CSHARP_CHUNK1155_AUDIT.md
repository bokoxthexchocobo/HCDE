# Chunk 1155 — Tid, group, and sector teleports from `p_teleport.cpp` (**644–806**).

## Scope

Tid, group, and sector teleports from `p_teleport.cpp` (**644–806**).

## Native ranges

- `src/playsim/p_teleport.cpp` **Group and sector teleport** 644–806 (163 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmPTeleportGroup1155Slice.cs`
Managed LOC (tally): 238.
Conversion commit: `ee8e53561355f14cf1f1144a1977876e3c2c9e7e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `EV_TeleportOther` (**651–667**) | yes |
| `DoGroupForOne` (**676–692**) | yes |
| `EV_TeleportGroup` (**701–751**) | yes |
| `EV_TeleportSector` (**761–806**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **471–643** | silent line teleport already credited |
| **681** | `offAngle` is computed and never read |

`TELF_DESTFOG` is **1**, `TELF_SOURCEFOG` is **2**, and `TELF_KEEPORIENTATION` is **4**. `ONFLOORZ` is signed **0x80000000**. `nullAngle` is **0** degrees. `EV_TeleportOther` does nothing unless both tids are non-zero. Fog uses dest and source fog without keep-orientation; otherwise the call keeps orientation. Each victim's host result is OR-ed. `DoGroupForOne` rotates the victim's XY offset by `dest.yaw - source.yaw` with degree cosine and sine, then adds the destination XY. Floor placement stores `ONFLOORZ`; otherwise Z is `dest.Z + victim.Z - source.Z`. Fog adds both fog bits and keep-orientation. The teleport angle is `nullAngle`, and the victim yaw is then `(dest.yaw + victim.yaw - source.yaw)` normalized to **360** degrees even when the teleport fails. A missing source origin makes `EV_TeleportGroup` call `EV_TeleportOther` with the group tid. A missing TeleportDest origin returns false. `TeleportDest2` keeps the relative Z; any other dest uses the floor. Group tid **0** with a victim moves that one actor; otherwise every iterator actor is moved. The source itself moves only after some victim succeeded, and only when `moveSource` is set. That source teleport keeps orientation with no fog bits, uses the dest XY, and copies the dest yaw without a second normalize. `EV_TeleportSector` returns false without a source or a TeleportDest. It walks each tagged sector's touching list, remembering the next node first, skips null actors, and skips a tid that does not match a non-zero group.

| Managed (`VmPTeleportGroup1155Slice.cs`) | Native (`p_teleport.cpp`) |
|------------------------------------------|----------------------------|
| Other, group, sector | **644–806** |
| Band hull (exclusive slice) | **644–806** |

### Chunk **1155** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **163** |
| **Net headline** | **+163** vs post-**1154** |


**Post-review ratio:** 238/163 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmPTeleportGroup1155SliceTests.Other_RequiresBothTidsAndDropsKeepOrientationWhenFogging`
- `VmPTeleportGroup1155SliceTests.MoveOne_RotatesAroundTheSourceAndNormalizesYaw`
- `VmPTeleportGroup1155SliceTests.GroupAndSector_FallBackAndFilterTouches`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1155 --to 1155` (0 hard / 0 soft).

## Seams

Actor iterators, the inner `P_Teleport` / `EV_Teleport` calls, and the TeleportDest class check stay host-supplied.

## Credit rationale

UNCredited **`p_teleport.cpp`** **644–806** (+163), the tail of the 806-line file, with no conversion or recognition overlap.
