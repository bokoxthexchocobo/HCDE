# Chunk 769 — `HandleOverlayRelative` and overlay vertex/scale/rotate/pivot/offset/flags actions (`p_pspr.cpp` 835–1084).

## Scope

`HandleOverlayRelative` and overlay vertex/scale/rotate/pivot/offset/flags actions (`p_pspr.cpp` 835–1084).

## Native ranges

- `src/playsim/p_pspr.cpp` **HandleOverlayRelative** 835–844 (10 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayVertexOffset)** 852–875 (24 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayScale)** 883–908 (26 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayRotate)** 916–935 (20 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayPivot)** 943–970 (28 lines)
- `src/playsim/p_pspr.cpp` **A_OverlayOffset** 978–1006 (29 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayOffset)** 1008–1017 (10 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_WeaponOffset)** 1019–1027 (9 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayFlags)** 1035–1062 (28 lines)
- `src/playsim/p_pspr.cpp` **DEFINE_ACTION_FUNCTION(AActor, A_OverlayPivotAlign)** 1064–1084 (21 lines)

**Credited native LOC:** 205 (tally `nativeLoc` 205).

## Managed

`VmPPSpriteOverlay.cs`
Managed LOC (tally): 149.
Conversion commit: `c2db57e528ee481b32d4d3f0e7879b8e4be517a9`.

## Padding review (2026-10-21)

| Managed | Native (`p_pspr.cpp`) |
|---------|----------------------|
| `HandleOverlayRelative` | **835–844** |
| `OverlayVertexOffset` | DEFINE **852–875** |
| `OverlayScale` | **883–908** |
| `OverlayRotate` | **916–935** |
| `OverlayPivot` | **943–970** |
| `OverlayOffset` | **978–1006**, DEFINE **1008–1017** |
| `WeaponOffsetAction` | DEFINE `A_WeaponOffset` **1019–1027** |
| `OverlayFlags` / `OverlayPivotAlign` | DEFINE **1035–1084** |


**Post-review ratio:** 149/205 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmPPSpriteOverlayTests.HandleOverlayRelative_RotatesOffset`
- `VmPPSpriteOverlayTests.OverlayOffset_AdditiveRelativeUpdatesPosition`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 769 --to 769` (0 hard / 0 soft).

## Seams

`ACTION_CALL_FROM_PSPRITE`, `FindPSprite`, and `stateinfo->mPSPIndex` are injected at action entry.

## Credit rationale

Faithful WOF_* flag handling for psprite overlay manipulation after chunk765 weapon bob cluster.
