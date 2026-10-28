# HCDE native-to-managed support matrix

Generated: 2026-10-28. Machine-readable: [`data/native_managed_matrix.json`](data/native_managed_matrix.json).

## LOC scope and tally

- Native engine scope: `src/` `*.cpp` / `*.c` only → **508,086** lines.
- Excluded from engine scope: libraries/ (vendored deps), tools/ (build utilities), protocol/ (shared constants; mirrored in HCDE.Protocol).
- Managed implementation: `csharp/src/**/*.cs` → **235,844** lines.

### Run tally

- Converted this run (native behavior, tested): **68,687** native lines.
- Recognition this run (already ported, tested): **-4,686** native lines.
- Added this run (managed): **93,041** C# lines.
- Cumulative conversion (native): **87,214**; recognition: **754**; combined: **87,968**.
- Native engine remaining (scope): **420,118** lines.

Native lines count toward conversion only when matching behavior is implemented and covered by regression tests.

## Headline coverage

| Area | Executed | Partial | Decoded-only | Absent |
| --- | ---: | ---: | ---: | ---: |
| ACS word opcodes (`AcsPcode`) | 313 | 10 | 51 | 0 |
| ZScript VM opcodes (`vmops.h` rows) | 246 | 0 | 0 | 0 |
| ACS `CallFunc` (`EACSFunctions` names) | 138 | 0 | 0 | 0 |

## Native `src/` LOC by top-level directory

| Directory | Native LOC | Managed notes |
| --- | ---: | --- |
| `src/common/` | 228,734 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/playsim/` | 88,088 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/rendering/` | 37,940 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/gamedata/` | 21,437 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/scripting/` | 15,536 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/maploader/` | 13,984 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/d_net.cpp/` | 10,667 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/r_data/` | 9,334 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/g_statusbar/` | 8,324 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/zcc-parse.c/` | 6,808 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/sound/` | 5,219 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/d_main.cpp/` | 5,108 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/widgets/` | 4,834 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/g_game.cpp/` | 3,813 | No full directory port; behavior landed incrementally in managed playsim/scripting. |
| `src/utility/` | 3,669 | No full directory port; behavior landed incrementally in managed playsim/scripting. |

## Method

- ACS opcode execution is inferred from `HCDE.Playsim/AcsVm.cs` switch cases (by `AcsPcode` wire index).
- ACS decode coverage is inferred from `HCDE.MapLoader/MapBehaviorBytecodeWalker.cs` operand tables.
- VM opcode names come from `src/common/scripting/vm/vmops.h`; execution from `HCDE.Scripting/VmExec.cs` (by opcode index).
- ACS `CallFunc` rows use native `EACSFunctions` wire indices vs `AcsCallFunctions.TryInvoke` dispatch slots.
- Partial ACS pcode rows are listed in `ACS_PARTIAL` inside `csharp/tools/generate_native_managed_matrix.py`.
