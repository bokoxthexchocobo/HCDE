# Chunk 1447 — NetUpdate packet bodies in d_net.cpp (7138-7389).

## Scope

NetUpdate packet bodies in d_net.cpp (7138-7389).

## Native ranges

- `src/d_net.cpp` **NetUpdate** 7138–7389 (252 lines)

**Credited native LOC:** 252 (tally `nativeLoc` 252).

## Managed

`VmNet1447Slice.cs`
Managed LOC (tally): 355.
Conversion commit: `d2a8eb598e8ee59f7e29482a3bdb5d85d38c76c5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Packet bodies (**7138–7389**) | yes |
| Band hull (**7138–7389**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Send window | lines 6940-7136 are already credited |
| Stability tail | the include at line 7390 opens a different function |

Quitters are written on the first packet only. A self destination sends its whole tic window in one loop. Held yaw is packed again even when it matches the previous tic. A native snapshot or client-input send replaces the live packet, and an extra tic repeats that send. In-game quitters are disconnected after the loop, and a resend-only pass leaves the last consistency stamp alone.

| Managed (`VmNet1447Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| WriteBodies | **7138–7389** |
| Band hull (exclusive slice) | **7138–7389** |

### Chunk **1447** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **252** |
| **Net headline** | **+252** vs the prior chunk |


**Post-review ratio:** 355/252 managed/native ≈ 141% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1447SliceTests.AuthorityBodyQuittersAndCursor`
- `VmNet1447SliceTests.ChannelsCursorAndHeldYaw`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1447 --to 1447` (0 hard / 0 soft).

## Seams

Consistency base through the trailing GetPackets. The send window ends at line 7136.

## Credit rationale

UNCredited d_net.cpp 7138-7389 (+252). One contiguous span of the packet loops and the quitter teardown, ending at the NetUpdate brace. The span does not cross credited code.
