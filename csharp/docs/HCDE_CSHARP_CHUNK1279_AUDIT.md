# Chunk 1279 — Profile apply, capability notes, and the preset reset from hw_k8vavoom_lighting.cpp (1-277).

## Scope

Profile apply, capability notes, and the preset reset from hw_k8vavoom_lighting.cpp (1-277).

## Native ranges

- `src/common/rendering/hwrenderer/data/hw_k8vavoom_lighting.cpp` **HCDE_K8vavoomApplyLightingProfile** 1–277 (277 lines)

**Credited native LOC:** 277 (tally `nativeLoc` 277).

## Managed

`VmK8Lighting1279Slice.cs`
Managed LOC (tally): 277.
Conversion commit: `b87a846436d3995ef75bc074d7e51fb7d976accf`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Apply profile (**80–141**) | yes |
| Capability probe (**149–180**) | yes |
| Prepare (**182–200**) | yes |
| Profile callback (**206–216**) | yes |
| Status (**218–254**) | yes |
| Reset (**256–277**) | yes |
| Band hull (**1–277**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Framebuffer queries | host device |
| Hardware-renderer flag | host video mode |

A profile at or below zero clears the snapshot and returns before any cvar write. A positive profile forces the shadow map, priority, bloom, budget, and fallback on. Quality rises only when it is below 1024 with the boost, or below 512 without it. A zero tonemap or ambient-occlusion value becomes one, and a nonzero value stays, including a negative one. A zero light cap becomes 512 or 256. A zero filter becomes one. Ray tracing is enabled only when the passed device can query and the flag is still off. The snapshot copies the live values after those writes, and its ray flag is the boost or the probe argument. Assigning the profile stores the value and then runs the callback. A value below zero or above one assigns the clamped value, which runs the callback again, and the outer call still applies. The auto profile turns off only when the stored value is already zero and the previous value was positive, so a negative assignment that clamps to zero leaves auto on. Prepare returns for a null device or a software renderer. On capable hardware with auto still on and the profile still zero, the boost follows ray-query support, the probe flag is set only when queries exist, and the profile assignment applies before the auto line. A later ray enable uses the passed device, which can differ from the global screen used inside apply. The status text prints the snapshot beside the live cvars and the first matching capability note. Reset assigns profile zero, then writes the default cvars and a fresh snapshot. The live priority and fallback stay on while that snapshot stays off.

| Managed (`VmK8Lighting1279Slice.cs`) | Native (`hw_k8vavoom_lighting.cpp`) |
|--------------------------------------|-------------------------------------|
| Profile, probe, and reset | **1–277** |
| Band hull (exclusive slice) | **1–277** |

### Chunk **1279** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **277** |
| **Net headline** | **+277** vs post-**1278** |


**Post-review ratio:** 277/277 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmK8Lighting1279SliceTests.Profile_ClampsOutsideTheRangeAndRaisesTheShadowFloor`
- `VmK8Lighting1279SliceTests.Prepare_UsesThePassedFramebufferAndKeepsTheNoteOrder`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1279 --to 1279` (0 hard / 0 soft).

## Seams

The framebuffer, hardware-renderer flag, and ray-query support are host inputs. The slice keeps the reentered profile callback, the shadow floor, the note order, and the reset that does not restore the snapshot flags.

## Credit rationale

UNCredited hw_k8vavoom_lighting.cpp 1-277 (+277). A full tally line scan shows no overlap. The file ends at line 277.
