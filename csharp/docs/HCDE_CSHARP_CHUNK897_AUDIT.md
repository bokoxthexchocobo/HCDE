# Chunk 897 — `A_SeekerMissile`, `DoTracer2`, `A_Tracer2`, and `A_Tracer` (`p_actionfunctions.cpp` **730–893**). Follows **739** pointer/sound (**728**); stops before `A_BulletAttack` (**894+**). `CheckIfSeen` in the same managed file targets **1854+** (uncredited; deferred).

## Scope

`A_SeekerMissile`, `DoTracer2`, `A_Tracer2`, and `A_Tracer` (`p_actionfunctions.cpp` **730–893**). Follows **739** pointer/sound (**728**); stops before `A_BulletAttack` (**894+**). `CheckIfSeen` in the same managed file targets **1854+** (uncredited; deferred).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_SeekerMissile** 730–893 (164 lines)

**Credited native LOC:** 164 (tally `nativeLoc` 164).

## Managed

`VmPActionSeekerTracer.cs`
Managed LOC (tally): 166.
Conversion commit: `40c40a1d56d57b2311f32e5b5ae891b8357a6124`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `Smf*` / `SeekerMissile` | `A_SeekerMissile` **742–781** |
| `DoTracer2` | `DoTracer2` **791–837** |
| `TracerAction` | `A_Tracer2` **839–845**, `A_Tracer` **853–887** |
| (seams) | `pr_seekermissile` **735**, `SMF_*` **736–741** |

### vs **739** / bullet band

| | Lines |
|--|------:|
| **739** (`A_StopSoundEx`) | ends **728** |
| **897** (this row) | **730–893** (**164**) |
| `A_BulletAttack` | **894+** |

### Deferred in file (not this row)

| Managed | Native |
|---------|--------|
| `CheckIfSeen` | **1854–1878** (uncredited) |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **164** |
| Net behavior-ported | **164** |


**Post-review ratio:** 166/164 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmPActionSeekerTracerTests.SeekerMissile_StickyPlayerTracerWhenNoLook`
- `VmPActionSeekerTracerTests.SeekerMissile_LookClearsTracerOnMiss`
- `VmPActionSeekerTracerTests.DoTracer2_SkipsDeadTracer`
- `VmPActionSeekerTracerTests.DoTracer2_TurnsTowardTracer`
- `VmPActionSeekerTracerTests.TracerAction_SkipsOffTic`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 897 --to 897` (0 hard / 0 soft).

## Seams

`pr_seekermissile`, `P_RoughMonsterSearch`, `P_SeekerMissile`, `Spawn`, `P_SpawnPuff`, and `VelFromAngle` are not hosted.

## Credit rationale

SMF sticky-tracer policy, seeker turn clamps, tracer steering, vertical chase, and tic-gated smoke/puff orchestration mirrored in `VmPActionSeekerTracer`.
