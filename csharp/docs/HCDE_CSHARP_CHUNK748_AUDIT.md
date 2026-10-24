# Chunk 748 — TID used/unique search, missile damage, howl, skullfly slam (`p_mobj.cpp` 3629–3827).

## Scope

TID used/unique search, missile damage, howl, skullfly slam (`p_mobj.cpp` 3629–3827).

## Native ranges

- `src/playsim/p_mobj.cpp` **FLevelLocals::IsTIDUsed** 3629–3641 (13 lines)
- `src/playsim/p_mobj.cpp` **FLevelLocals::FindUniqueTID** 3654–3700 (47 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetMissileDamage** 3720–3751 (32 lines)
- `src/playsim/p_mobj.cpp` **AActor::Howl** 3753–3760 (8 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, Howl)** 3762–3767 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::Slam** 3769–3805 (37 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, Slam)** 3807–3812 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::CallSlam** 3814–3827 (14 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmPMobjTidMissileSlam.cs`
Managed LOC (tally): 208.
Conversion commit: `9f20ef8c7d588f61beb9609f3bde681eeedc5b2d`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `IsTidUsed` / `TidHashIndex` | `FLevelLocals::IsTIDUsed` 3629–3641 |
| `FindUniqueTid` / `FindUniqueTidLinear` / `FindUniqueTidRandomBatch` / `ClampLinearLimit` | `FLevelLocals::FindUniqueTID` 3654–3700 |
| `GetMissileDamage` | `AActor::GetMissileDamage` 3720–3751 |
| `Howl` | `AActor::Howl` 3753–3760 |
| `Howl` | `DEFINE_ACTION_FUNCTION(AActor, Howl)` 3762–3767 |
| `Slam` / `OnlySlamSolidPass` | `AActor::Slam` 3769–3805 |
| `Slam` | `DEFINE_ACTION_FUNCTION(AActor, Slam)` 3807–3812 |
| `CallSlam` | `AActor::CallSlam` 3814–3827 |


**Post-review ratio:** 208/163 managed/native ≈ 128% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjTidMissileSlamTests.IsTidUsed_FindsMatchingTid`
- `VmPMobjTidMissileSlamTests.FindUniqueTid_LinearSearch`
- `VmPMobjTidMissileSlamTests.GetMissileDamage_ConstantDamage`
- `VmPMobjTidMissileSlamTests.Howl_SkipsWhenAlreadyPlaying`
- `VmPMobjTidMissileSlamTests.Slam_OnlySlamSolidPass`
- `VmPMobjTidMissileSlamTests.CallSlam_UsesVirtualWhenPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 748 --to 748` (0 hard / 0 soft).

## Seams

`TIDHash`/`ClientSideTIDHash`, `VMCall`, `P_DamageMobj`, `P_TraceBleed`, `SetState`, and `S_Sound` are injected.

## Credit rationale

Whole p_mobj helpers after TID hash band (chunk747); excludes CCMD `utid`.
