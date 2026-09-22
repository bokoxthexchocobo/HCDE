# Phase 4 audit — client subset

**Date:** 2026-09-22  
**Scope:** Headless-client gates from [`HCDE_CSHARP_MIGRATION.md`](HCDE_CSHARP_MIGRATION.md) Phase 4: a managed ZScript stand-in, a software column view, a recorded hardware blit, a PCM mixer, and a launcher argument plan.  
**C++ references:** `src/common/scripting/vm/vmexec.cpp` (the real VM), `src/rendering/swrenderer/` (the software renderer), `libraries/ZVulkan` and `vulkan-1` (hardware), `libraries/ZMusic` (audio), `libraries/asmjit` (JIT).  
**Result:** Accepted for this subset. Not a port of those trees, and not a windowed client.

`dotnet test` in `csharp/`: **624 passed, 0 failed.** That is the Phase 3 total of 612 plus 12 Phase 4 tests. Local SDK is 10.0.201; `global.json` still pins 8.0.423, so the run moved that file aside and put it back. The projects still target `net8.0`.

`HCDE.Client` (`hcde`) and `HCDE.Client.Tests` are in `HCDE.sln`.

## What a client tick does

`ClientHost.Tick` drives the Phase 3 simulation. It does not open a socket or a window.

1. Queue the command on the first player, if one exists.
2. `AuthoritySimulation.Tick`.
3. Resume ZScript fibers.
4. Draw. The software path fills ceiling and floor, then paints blocking lines. The hardware path clears the frame, records one wall-colored quad, and blits it on the CPU.
5. Queue a four-sample click and mix it into `LastMix`.

`DemoRoom` is the oracle: player start type 1 at (32, 64), one one-sided vertical line at x=64 from y=0 to y=128. Angle 0 faces east. A 32×32 frame's center column hits that wall.

## Gates

### ZScript

Opcodes: nop, load immediate (int32), add, jump-if-zero (absolute pc), return, call-native (int32 id). Natives: log the stack top, push player health, pop an amount and subtract it from health (clamped at 0; a negative amount does not heal). An unknown opcode or an unknown native ends the fiber. A missing program does not start. The stack underflow ends the fiber. Each resume stops after 64 instructions and leaves the fiber running.

asmjit is probed with `NativeLibrary.TryLoad` and then freed. `Bound` stays false. Nothing is JIT-compiled.

Checked: load 10 and damage the player to 90. Load 2, load 3, add, log `"5"`. A missing name returns false. Opcode 255 stops. Jump-if-zero over a 50-point damage leaves health at 100 and logs `"100"`. Native id 99 stops. A nop / load 0 / jump-to-start loop is still running after one tick.

Left out: `vmexec.cpp`, the compiler, strings, objects, states, inventory, ACS compatibility, and any call into asmjit.

### Software view

One ray per column, 90° field of view, from the player angle. Only lines with `BlocksMovement` (one-sided, or the blocking flag) are tested. The nearest hit with distance > 0.05 and a hit fraction in [0, 1] becomes a solid column. Column height is `frame height * 16 / distance`, clamped to the frame, and centered. Ceiling is (40, 40, 80). Floor is (40, 30, 20). Wall is (180, 40, 40). A column with no hit keeps the ceiling/floor fill. Pixels above the horizon are ceiling.

Checked: the demo room's top center pixel is ceiling and the horizon center is wall. A wall at x=0, behind a player facing east, leaves the horizon pixel as floor.

Left out: `rendering/swrenderer` — spans, sprites, planes, portals, colormap, texture columns, and the real projection. The column scale is a constant, not the C++ focal length.

### Hardware path

`RecordedHardwareDevice` stores quads. `BeginFrame` clears them. `DrawQuad` throws if the frame is not open. `Blit` writes `0x00RRGGBB` into the software frame and closes the frame. `IsNative` is false. The client hardware tick paints one full-width quad, `HardwareWall` `0x00B42828`, from y = height/4 with height/2.

Vulkan is probed as `vulkan-1` on Windows and `vulkan` elsewhere, then freed. `Bound` stays false. No instance, device, or submit exists.

Checked: one recorded quad, center pixel (180, 40, 40), Vulkan unbound, and a draw after blit throws.

Left out: Silk.NET, a Vulkan instance, shaders, swapchain, and the C++ hardware scene.

### Audio

`AudioMixer` copies each PCM buffer, sums overlapping samples, and clamps to int16. `Mix` then drops the queued buffers. Mute still drops them and returns zeros. An empty buffer is ignored. Sample rate defaults to 11025 and is not used to resample. The client tick always queues `{1000, 1000, 1000, 1000}` and mixes four samples.

ZMusic is probed and freed. `Bound` stays false. No song, sound, or MIDI handle is opened.

Checked: 20000+20000 clamps to `short.MaxValue`, and the negative pair clamps to `short.MinValue`. Mute yields 0. A client tick's first mixed sample is 1000. ZMusic is unbound.

Left out: `libraries/ZMusic`, timidity, fluidsynth, and the Eternity reverb that Phase 3 only flags.

### Launcher

`LauncherSelection` requires an IWAD path, a map name, skill 1–5, and a view at least 16×16. `BuildArgs` emits `--iwad`, `--map`, `--skill`, `--width`, `--height`, and `--renderer software|hardware`. `TryParse` rejects an unknown flag, a missing value, and a renderer other than those two. `hcde` with no arguments prints usage and returns 2. `--help` returns 0. `--self-test` builds the demo room, ticks a default command, prints `center=R,G,B tic=N`, and returns 0 only when the center pixel is the wall color. Other arguments are parsed and printed. The process does not load an IWAD or create a window.

Checked: skill 0 is `skill-range`. A hardware / MAP07 / skill 4 selection round-trips through `BuildArgs` and `TryParse`. `--self-test` returns 0 and prints `center=180,40,40`.

Left out: Avalonia, ImGui, IWAD discovery, and launching the C++ client.

## Verdict

Phase 4's client gates are in and tested. The software renderer, Vulkan submission, ZMusic playback, asmjit, the ZScript compiler, launcher widgets, IWAD boot, and a network client are not replaced. The C++ sources stay. The next playsim slice is [`HCDE_CSHARP_PHASE5_AUDIT.md`](HCDE_CSHARP_PHASE5_AUDIT.md).
