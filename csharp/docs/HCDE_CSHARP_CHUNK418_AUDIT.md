# Chunk 418 — ACS execute/suspend, FS execute, LowerRaise boom gate, light line specials

## Scope

ACS execute/suspend, FS execute, LowerRaise boom gate, light line specials

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_ACS_*** 1905–2005 (101 lines)
- `src/playsim/p_lnspec.cpp` **LS_FS_Execute** 2013–2019 (7 lines)
- `src/playsim/p_lnspec.cpp` **LS_FloorAndCeiling_LowerRaise** 2033–2046 (14 lines)
- `src/playsim/p_lnspec.cpp` **LS_Light_*** 2066–2148 (83 lines)

**Credited native LOC:** 205 (tally `nativeLoc` 205).

## Managed

`VmPLnspecAcsLineSpecials.cs`, `VmPLnspecAcsLineSpecialsTests.cs`, `VmPLnspecLightLineSpecials.cs`, `VmPLnspecLightLineSpecialsTests.cs`
Managed LOC (tally): 200.
Conversion commit: `a70048c317fc5c92795ada8642c7e63eaadd1bb2`.

## Tests

- `VmPLnspecAcsLineSpecialsTests.AcsExecute_UsesCurrentMapWhenZero`
- `VmPLnspecAcsLineSpecialsTests.LockedExecute_BlocksWithoutKey`
- `VmPLnspecAcsLineSpecialsTests.Suspend_UsesCurrentMapWhenZero`
- `VmPLnspecAcsLineSpecialsTests.FsExecute_RejectsBacksideWhenFirstSideOnly`
- `VmPLnspecAcsLineSpecialsTests.LowerRaise_RunsBoomFloorWhenCeilingFails`
- `VmPLnspecLightLineSpecialsTests.LightFade_UsesTicsMacro`
- `VmPLnspecLightLineSpecialsTests.LightLower_NegatesValue`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 418 --to 418` (0 hard / 0 soft).

## Seams

`P_StartScript`, `T_RunScript`, `EV_*` light movers, and `ForceLightning` are not hosted.

## Credit rationale

Credits ACS map resolution, locked execute gates, FS execute checks, Boom `LowerRaise` floor fallback, and `LS_Light_*` argument wiring from `p_lnspec.cpp`.
