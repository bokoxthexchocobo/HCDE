# Chunk 472 — AbsorbDamage/AlterWeaponSprite inventory walks, active/push sounds, GetBloodType, portal transition

## Scope

AbsorbDamage/AlterWeaponSprite inventory walks, active/push sounds, GetBloodType, portal transition

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::AbsorbDamage** 3904–3918 (15 lines)
- `src/playsim/p_mobj.cpp` **AActor::AlterWeaponSprite** 3920–3937 (18 lines)
- `src/playsim/p_mobj.cpp` **AActor::PlayActiveSound** 3939–3953 (15 lines)
- `src/playsim/p_mobj.cpp` **AActor::PlayPushSound** 3955–3969 (15 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetBloodType** 4203–4214 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetPortalTransition** 4216–4247 (32 lines)
- `src/playsim/p_mobj.cpp` **AActor::CheckPortalTransition** 4249–4294 (46 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetModelTimer** 4296–4306 (11 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmPMobjInventorySoundPortal.cs`, `VmPMobjInventorySoundPortalTests.cs`
Managed LOC (tally): 123.
Conversion commit: `b090291c396d21320b3f349d76716745c02210d8`.

## Tests

- `VmPMobjInventorySoundPortalTests.ReverseInventoryOrder_WalksBackwards`
- `VmPMobjInventorySoundPortalTests.PlayActiveSound_RequiresFreeVoice`
- `VmPMobjInventorySoundPortalTests.PlayActiveSound_BlocksWhenBusy`
- `VmPMobjInventorySoundPortalTests.ActiveAttenuation_FullVolumeFlag`
- `VmPMobjInventorySoundPortalTests.PlayPushSound_WhenBodyFree`
- `VmPMobjInventorySoundPortalTests.ModelTimer_ClientUsesLocal`
- `VmPMobjInventorySoundPortalTests.GetPortalTransition_MovesThroughCeiling`
- `VmPMobjInventorySoundPortalTests.CheckPortal_CeilingStepWhenAtPlane`
- `VmPMobjInventorySoundPortalTests.CheckPortal_FloorStepWhenBelowPlane`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 472 --to 472` (0 hard / 0 soft).

## Seams

`VMCall`, `S_Sound`, `S_IsActorPlayingSomething`, sector portal hosting, and `LinkToWorld` are not hosted.

## Credit rationale

Credits inventory reverse walks, active/push sound gates, virtual blood-type lookup, and ceiling/floor portal transition predicates from `p_mobj.cpp` (3904–3962, 4203–4306).
