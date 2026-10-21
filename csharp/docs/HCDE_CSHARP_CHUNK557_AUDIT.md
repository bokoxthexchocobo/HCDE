# Chunk 557 — Anim frame queries, bone frame pose, base position/rotation, GetBoneTRS (5890–6147)

## Scope

Anim frame queries, bone frame pose, base position/rotation, GetBoneTRS (5890–6147)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **GetAnimStartFrameNative** 5890–5895 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetAnimStartFrame** 5897–5903 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **GetAnimEndFrameNative** 5905–5910 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetAnimEndFrame** 5912–5918 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **GetAnimFramerateNative** 5920–5925 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetAnimFramerate** 5927–5933 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetBoneFramePose** 5935–5973 (39 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneFramePose** 5975–6015 (41 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetBoneBasePosition** 6017–6025 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneBasePosition** 6027–6037 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetBoneBaseRotation** 6039–6047 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneBaseRotation** 6049–6059 (11 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetBoneTRS** 6067–6105 (39 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneTRS** 6107–6147 (41 lines)

**Credited native LOC:** 239 (tally `nativeLoc` 239).

## Managed

`VmActorBonePoseGetters.cs`
Managed LOC (tally): 236.
Conversion commit: `e51d094e27a534efc42af50e9da9691f2c1e013d`.

## Padding review (2026-10-21)

Whole-function ports only; measured after faithful port (no LOC target).

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `GetAnimStartFrameNative`, `GetAnimStartFrameActionWrapper` | 5890–5903 |
| `GetAnimEndFrameNative`, `GetAnimEndFrameActionWrapper` | 5905–5918 |
| `GetAnimFramerateNative`, `GetAnimFramerateActionWrapper` | 5920–5933 |
| `GetBoneFramePoseNative` | `GetBoneFramePose` 5935–5973 |
| `GetNamedBoneFramePoseNative` | `GetNamedBoneFramePose` 5975–6015 |
| `GetBoneBasePositionNative` | `GetBoneBasePosition` 6017–6025 |
| `GetNamedBoneBasePositionNative` | `GetNamedBoneBasePosition` 6027–6037 |
| `GetBoneBaseRotationNative` | `GetBoneBaseRotation` 6039–6047 |
| `GetNamedBoneBaseRotationNative` | `GetNamedBoneBaseRotation` 6049–6059 |
| `GetBoneTrsNative` | `GetBoneTRS` 6067–6105 |
| `GetNamedBoneTrsNative` | `GetNamedBoneTRS` 6107–6147 |

**Post-review ratio:** 236/239 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmActorBonePoseGettersTests.GetAnimStartFrameNative_ReturnsFirstFrame`
- `VmActorBonePoseGettersTests.GetBoneFramePoseNative_CapsNumret`
- `VmActorBonePoseGettersTests.GetBoneBasePositionNative_ReturnsPosition`
- `VmActorBonePoseGettersTests.GetBoneTrsNative_UsesActorTrs`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 557 --to 557` (0 hard / 0 soft).

## Seams

`SetGetBoneShared`, `GetBoneShared`, `FModel` frame/pose queries, and `AActor::GetBoneTRS` are not hosted.

## Credit rationale

One managed method per credited whole native `static` body or full DEFINE implementation.
