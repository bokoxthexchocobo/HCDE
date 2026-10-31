# Chunk 1361 — Gyro cvars, SDL sensor probe, m_gyro_status, and ticcmd contribution from i_input_gyro.cpp (1-246).

## Scope

Gyro cvars, SDL sensor probe, m_gyro_status, and ticcmd contribution from i_input_gyro.cpp (1-246).

## Native ranges

- `src/i_input_gyro.cpp` **HCDEGyro** 1–246 (246 lines)

**Credited native LOC:** 246 (tally `nativeLoc` 246).

## Managed

`VmGyroInput1361Slice.cs`
Managed LOC (tally): 212.
Conversion commit: `12d7d0a8829ddb401f5d89cd19de9e2340c9bf6e`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Banner and includes (**1–33**) | yes |
| Platform sample and the SDL probe (**34–134**) | yes |
| Mode gate (**136–146**) | yes |
| Gyro cvars (**149–171**) | yes |
| m_gyro_status (**173–191**) | yes |
| Ticcmd contribution (**193–234**) | yes |
| Scoped-out note (**236–246**) | yes |
| Band hull (**1–246**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| i_input_gyro.h | the declaration is a different file |

`joy_gyro_enable` defaults off. The scales default to 2.5 and 2. `joy_gyro_deadzone` defaults to 0.05. `joy_gyro_mode` defaults to 0. Both invert switches default off.

`HCDEGyroModeAllowsContribution` returns true when the mode is less than or equal to 0, including a negative mode. Held mode 1 and toggle mode 2 return false.

`HCDEGyro_PollPlatformSample` starts from a zero sample whose provider is `none`. A disabled cvar sets the provider to `disabled` and does not query a platform. A build that is not Windows sets `platform-sensor-probe-unavailable` and leaves `PlatformQueried` false. The Windows path sets `PlatformQueried`, then looks up `SDL2.dll` with `GetModuleHandleA` and, only when that is null, `LoadLibraryA`. A missing module sets `windows-sdl2-not-loaded`. All eight sensor symbols are resolved before the missing-symbol check. A missing symbol sets `windows-sdl2-sensor-symbols-missing` and does not call `SDL_WasInit`. `SDL_InitSubSystem` runs only when `SDL_WasInit` does not already contain `SDL_INIT_SENSOR` (0x8000). A nonzero init result sets `windows-sdl2-sensor-init-failed` and does not update sensors. Otherwise the probe stores `SDL_NumSensors`, skips non-gyro devices (`SDL_SENSOR_GYRO` is 2), skips a null open, closes a sensor after `SDL_SensorGetData`, and keeps scanning when that call is nonzero. The first success stores pitch from value 0 and yaw from value 1, ignores value 2, and returns. No match leaves the provider at `windows-sdl2-dynamic-sensor`.

`m_gyro_status` polls once, then prints the cvars with `%.3f`, the mode parenthetical, the sample, and whether the mode allows a contribution. That last line follows the mode only. Enable can be off while the line still says yes.

`HCDEGyro_GetTiccmdContribution` writes zero to both outputs first. Disable returns before the mode check. A blocked mode returns before the poll. An unavailable sample returns before the math. Deadzone uses a strict inside test, so a rate equal to the deadzone is kept, and a negative deadzone does not zero a finite rate. Scale is applied after the deadzone. Invert is applied after the scale.

| Managed (VmGyroInput1361Slice.cs) | Native (i_input_gyro.cpp) |
|-----------------------------------|---------------------------|
| Cvars through the ticcmd contribution | **1–246** |
| Band hull (exclusive slice) | **1–246** |

### Chunk **1361** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **246** |
| **Net headline** | **+246** vs the prior chunk |


**Post-review ratio:** 212/246 managed/native ≈ 86% (src file LOC at HEAD; see tally).

## Tests

- `VmGyroInput1361SliceTests.DefaultsStatusAndContribution`
- `VmGyroInput1361SliceTests.WindowsSensorProbe`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1361 --to 1361` (0 hard / 0 soft).

## Seams

SDL2.dll lookup stays a host hook. i_input_gyro.h only declares HCDEGyro_GetTiccmdContribution and is not this file.

## Credit rationale

UNCredited i_input_gyro.cpp 1-246 (+246). A full tally line scan, including recognition ranges, shows no overlap. This is the largest remaining hole of at most 400 lines. It is not a tie. g_cvars.cpp 1-244 is 244. Gaps longer than 400 lines stay whole and are not sliced.
