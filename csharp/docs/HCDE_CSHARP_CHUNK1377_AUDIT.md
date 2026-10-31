# Chunk 1377 — Door sector construct, spawn construct, and EV_DoDoor from a_doors.cpp (343-547).

## Scope

Door sector construct, spawn construct, and EV_DoDoor from a_doors.cpp (343-547).

## Native ranges

- `src/playsim/mapthinkers/a_doors.cpp` **DDoor::Construct** 343–547 (205 lines)

**Credited native LOC:** 205 (tally `nativeLoc` 205).

## Managed

`VmDoorConstruct1377Slice.cs`
Managed LOC (tally): 269.
Conversion commit: `c192f1e06d56945630ef19e6b96664fe93f4bac4`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Blank line after `DoorSound` (**343**) | yes |
| One-argument `DDoor::Construct` (**344–347**) | yes |
| Spawn comment (**349–353**) | yes |
| Spawn `DDoor::Construct` (**355–429**) | yes |
| `EV_DoDoor` comment and body (**431–537**) | yes |
| Animated-door banner and `IMPLEMENT_CLASS` (**539–545**) | yes |
| `DAnimatedDoor::Construct` signature (**547**) | yes |
| Band hull (**343–547**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| `DDoor::DoorSound` **246–342** | chunk 793 |
| `DAnimatedDoor::Construct` body **548–551** | chunk 795 |
| Plane and key helpers | callees |

The one-argument construct only calls `Super::Construct`. The spawn construct stores speed, delay, countdown, and light tag, then clears the light tag when `COMPATF_NODOORLIGHT` is set. Close, open, raise, and wait-raise take the lowest surrounding ceiling minus 4. Open and raise play the raise sound only when that distance differs from the current ceiling. Close-wait-open copies the ceiling distance and plays the close sound. Wait-close forces type `doorRaise`, direction 0, and the current ceiling distance. The floor block then overwrites the bottom distance: highest floor unless the floor thinker is a lifting plat, in which case the lowest ceiling. `OldFloorDist` is always the floor distance after that block. A manual door with no back side plays `*usefail` on `CHAN_VOICE` at volume 1 and `ATTN_NORM`. A moving boom door returns without reuse. A closing raise door is sent back up and the call still returns false. A non-push activation closes a raise door for a real player when the repeat cooldown does not apply, and that path returns true. Push and monster-push activations do not close. A remote tag walks every sector, skips moving ceilings, and the trace uses the last sector in the iterator. `DAnimatedDoor` is a concrete class with no pointer offsets.

| Managed (`VmDoorConstruct1377Slice.cs`) | Native (`a_doors.cpp`) |
|-----------------------------------------|------------------------|
| Sector construct, spawn construct, and `EV_DoDoor` | **343–547** |
| Band hull (exclusive slice) | **343–547** |

### Chunk **1377** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **205** |
| **Net headline** | **+205** vs the prior chunk |


**Post-review ratio:** 269/205 managed/native ≈ 131% (src file LOC at HEAD; see tally).

## Tests

- `VmDoorConstruct1377SliceTests.ConstructAppliesTypeLightAndBotDist`
- `VmDoorConstruct1377SliceTests.EvDoDoorChecksKeysReuseAndRemote`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1377 --to 1377` (0 hard / 0 soft).

## Seams

FindLowestCeilingSurrounding, FindHighestFloorPoint, FindLowestCeilingPoint, PointToDist, DoorSound, P_CheckKeys, PlaneMoving, CreateThinker, the sector-tag iterator, SN_CheckSequence, and the player-index subtraction are callees. COMPATF_NODOORLIGHT is 1<<9. SPAC_Push is 1<<4 and SPAC_MPush is 1<<9. CHAN_CEILING is CHAN_VOICE (2). DAnimatedDoor::Construct body at 548 stays chunk 795.

## Credit rationale

UNCredited a_doors.cpp 343-547 (+205). After chunk 1376 this is the unique largest 150-400 hole, ahead of win32basevideo.cpp 1-204. Chunk 794 retired the earlier construct credit. Gaps longer than 400 lines stay whole and are not sliced.
