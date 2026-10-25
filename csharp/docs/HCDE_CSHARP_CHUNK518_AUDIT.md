# Chunk 518 — Net_TrySkipCommand

## Scope

Net_TrySkipCommand

## Native ranges

- `src/d_net.cpp` **Net_TrySkipCommand** 9261–9506 (246 lines)

**Credited native LOC:** 246 (tally `nativeLoc` 246).

## Managed

`VmDNetTrySkipCommand.cs`
Managed LOC (tally): 142.
Conversion commit: `17cc3a4b3238140e4e9edab359f0bf7af90d0185`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `TryReadCStringBytes` / `SkipCStringPlus` / `TryComputeSkip` / `TrySkipSay` / `TrySkipAddBot` / `TrySkipGiveCheat` / `TrySkipSetInv` / `TrySkipNetEvent` / `TrySkipZscCmd` / `TrySkipSummon2` / `TrySkipChangeMap2` / `TrySkipCStringOnly` / `TrySkipFixed` / `TrySkipRunScript` / `TrySkipRunNamedScript` / `TrySkipSinFChanged` / `TrySkipSetSlot` / `TrySkipAddSlot` / `CanAdvance` / `AdvanceCursor` / `UnknownCommand` | `Net_TrySkipCommand` 9261–9506 (9266: `if (offset > stream.Size())`; 9267: `return false;`; 9272: `if (terminator == nullptr)`) |

### Out-of-body uncredit (518)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **246** |
| **Out-of-body removed** | **0** |
| **Honest** | **246** |


**Post-review ratio:** 142/246 managed/native ≈ 58% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetTrySkipCommandTests.TryReadCStringBytes_FindsTerminator`
- `VmDNetTrySkipCommandTests.TrySkipZscCmd_AddsBigEndianLength`
- `VmDNetTrySkipCommandTests.CanAdvance_RejectsOverflow`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 518 --to 518` (0 hard / 0 soft).

## Seams

`AdvanceStream`, demo opcode tables, and `memchr` are not hosted.

## Credit rationale

Credits demo command skip sizing for SAY/TAUNT/cheat/ZSC/runscript paths and fail-closed unknown opcodes from `d_net.cpp` (9261–9506).
