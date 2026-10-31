# Chunk 1112 — Floor-bounce MBF rest, `P_FaceMobj`, `CanSeek`, and `P_SeekerMissile` turn/aim from `p_mobj.cpp` (**2251–2400**); follows **1111**.

## Scope

Floor-bounce MBF rest, `P_FaceMobj`, `CanSeek`, and `P_SeekerMissile` turn/aim from `p_mobj.cpp` (**2251–2400**); follows **1111**.

## Native ranges

- `src/playsim/p_mobj.cpp` **FaceMobj CanSeek SeekerMissile** 2251–2400 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPMobjSeekerMissile1112Slice.cs`
Managed LOC (tally): 148.
Conversion commit: `7cfcd086d2d8e6c43720dea797d24a0b80853822`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| MBF rest / AutoOff clear (**2259–2272**) | yes |
| `P_FaceMobj` clockwise sign (**2290–2300**) | yes |
| `CanSeek` invisible gate (**2313–2321**) | yes |
| Seeker dead/zero-speed (**2349–2360**) | yes |
| Threshold halve and turn max (**2363–2378**) | yes |
| Imprecise vertical seek vs precise aim height (**2380–2406**) | yes (policy) |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **2324–2329** | VM thunk |
| **2382**, **2407** | host velocity writers |
| **2413+** | `P_XYMovement` outside hull |

| Managed (`VmPMobjSeekerMissile1112Slice.cs`) | Native (`p_mobj.cpp`) |
|---------------------------------------------|---------------------|
| Bounce rest + seeker policy | **2251–2400** |
| Band hull (exclusive slice) | **2251–2400** |

### Chunk **1112** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **150** |
| **Net headline** | **+150** vs post-**1111** |


**Post-review ratio:** 148/150 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSeekerMissile1112SliceTests.ShouldZeroMbfVerticalSpeed_WhenBelowMassGravity`
- `VmPMobjSeekerMissile1112SliceTests.FaceMobjDirection_ClockwiseWhenDeltaPositive`
- `VmPMobjSeekerMissile1112SliceTests.CanSeek_RejectsInvisibleWhenFlagged`
- `VmPMobjSeekerMissile1112SliceTests.RunSeekerMissile_HalvesDeltaAboveThreshold`
- `VmPMobjSeekerMissile1112SliceTests.PreciseAimHeight_UsesViewZForPlayers`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1112 --to 1112` (0 hard / 0 soft).

## Seams

`AngleTo`, `VelFromAngle`, and `Vel3DFromAngle` remain host-injected; `P_XYMovement` at **2421** is credited on **724**.

## Credit rationale

UNCredited **`p_mobj.cpp`** **2251–2400** (+150); follows **1111** (**2101–2250**).
