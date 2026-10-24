# Chunk 565 — GetNetBufferSize minimum validation, HSendPacket demo skip, HGetPacket local buffer (5010–5195)

## Scope

GetNetBufferSize minimum validation, HSendPacket demo skip, HGetPacket local buffer (5010–5195)

## Native ranges

- `src/d_net.cpp` **GetNetBufferSize** 5010–5140 (131 lines)
- `src/d_net.cpp` **HSendPacket** 5145–5167 (23 lines)
- `src/d_net.cpp` **HGetPacket** 5171–5195 (25 lines)

**Credited native LOC:** 179 (tally `nativeLoc` 179).

## Managed

`VmDNetGetNetBufferSize.cs`
Managed LOC (tally): 104.
Conversion commit: `433399950d5135093c4bb75e6c9dd56388d0e0ca`.

## Padding review (2026-10-21)

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `GetNetBufferSizeNative` | `GetNetBufferSize` 5010–5140 |
| `HSendPacketShouldSkip` | `HSendPacket` demo guard 5147–5149 |
| `HGetPacketFromLocalBuffer` | `HGetPacket` local buffer branch 5176–5182 |

**Post-review ratio:** 104/179 managed/native ≈ 58% (src file LOC at HEAD; see tally).

## Tests

- `VmDNetGetNetBufferSizeTests.GetNetBufferSizeNative_ExitPacket_IncludesAuthorityByte`
- `VmDNetGetNetBufferSizeTests.HSendPacketShouldSkip_WhenDemoPlayback`
- `VmDNetGetNetBufferSizeTests.GetNetBufferSizeNative_LevelReady_AddsGameticBytesForAuthority`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 565 --to 565` (0 hard / 0 soft).

## Seams

`Net_TrySkipUserCmdMessageWithBoundary`, `I_NetCmd`, and live `NetBuffer` storage are not hosted.

## Credit rationale

Ports packet-size floor logic and send/receive guards; full command-stream walk remains native.
