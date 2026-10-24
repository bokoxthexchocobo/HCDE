# Chunk 518 — Net_TrySkipCommand

## Scope

Net_TrySkipCommand

## Native ranges

- `src/d_net.cpp` **Net_TrySkipCommand** 9261–9506 (246 lines)

**Credited native LOC:** 246 (tally `nativeLoc` 246).

## Managed

(see conversion commit file list)
Managed LOC (tally): 139.
Conversion commit: `e8a28c751fcd90873a2a60468d65a1b70f81de7a`.

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
