# Chunk 518 — Net_TrySkipCommand

## Scope

Net_TrySkipCommand

## Native ranges

- `src/d_net.cpp` **Net_TrySkipCommand** 9261–9506 (246 lines)

**Credited native LOC:** 246 (tally `nativeLoc` 246).

## Managed

`VmDNetTrySkipCommand.cs`
Managed LOC (tally): 424.
Conversion commit: `427b184767dbd133815ec94ece132c08689be29d`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `Dem*` constants / `TryReadCStringBytes` / `TryComputeCommandSkip` / `TrySkipCommand` / `TrySkip*FromStream` / `TrySkipCStringAtZero` / `TrySkipSaveGame` / `IsZeroPayloadCommand` / per-opcode `TrySkip*` helpers / `CanAdvance` / `AdvanceCursor` / `UnknownCommand` | `Net_TrySkipCommand` 9261–9506 (9264–9277: CString scan; 9280–9496: opcode switch; 9498–9499: default; 9502–9504: skip bound) |

### Out-of-body uncredit (518)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **246** |
| **Out-of-body removed** | **0** |
| **Honest** | **246** |


**Post-review ratio:** 424/246 managed/native ≈ 172% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetTrySkipCommandTests.TryReadCStringBytes_FindsTerminator`
- `VmDNetTrySkipCommandTests.TrySkipZscCmd_AddsBigEndianLength`
- `VmDNetTrySkipCommandTests.CanAdvance_RejectsOverflow`
- `VmDNetTrySkipCommandTests.TrySkipCommand_Say_skipsVariantByteAndCString`
- `VmDNetTrySkipCommandTests.TrySkipCommand_UnknownOpcode_fails`
- `VmDNetTrySkipCommandTests.TrySkipCommand_Suicide_zeroSkip`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 518 --to 518` (0 hard / 0 soft).

## Seams

`AdvanceStream`, demo opcode tables, and `memchr` are not hosted.

## Credit rationale

Credits demo command skip sizing for SAY/TAUNT/cheat/ZSC/runscript paths and fail-closed unknown opcodes from `d_net.cpp` (9261–9506).
