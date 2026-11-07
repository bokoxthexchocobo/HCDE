# Chunk 1441 — Authority handoff through level-start check in d_net.cpp (5457-5792).

## Scope

Authority handoff through level-start check in d_net.cpp (5457-5792).

## Native ranges

- `src/d_net.cpp` **SetArbitrator** 5457–5792 (336 lines)

**Credited native LOC:** 336 (tally `nativeLoc` 336).

## Managed

`VmNet1441Slice.cs`
Managed LOC (tally): 324.
Conversion commit: `4ffb4377fae8c440a9ebeae4e9c5a94f0407b465`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Authority handoff through level-start check (**5457–5792**) | yes |
| Band hull (**5457–5792**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Blank line and the next function's comment | lines 5793-5795 stay outside this span |
| Malformed-packet note and GetPackets | lines 5796-6227 stay outside this span |

The next authority is the first other client that is not the reserved slot, then any other client, then the reserved slot on a dedicated server, and otherwise the client that left. A quit from a client that is not the authority is ignored unless this process is the authority, in which case it is queued. An authority quit disconnects that client and seats a replacement that is in game, not the same client, and not reserved. A level starts only when every playable client has acked and the map is loaded. Release delay is the floor of the latency gap in seconds times 35, capped at 350 ms. A client that is not waiting resends the ready packet only for the local authority and only to someone other than the console player. An authority start aligns gametic, fast-forwards a dedicated joiner to the clamped prediction lead, and then pins an inflated client tic back to that lead. The applied sequence is then the lesser of the current sequence and gametic divided by the tic duplicate.

| Managed (`VmNet1441Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| SetArbitrator through CheckLevelStart | **5457–5792** |
| Band hull (exclusive slice) | **5457–5792** |

### Chunk **1441** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **336** |
| **Net headline** | **+336** vs the prior chunk |


**Post-review ratio:** 324/336 managed/native ≈ 96% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1441SliceTests.AuthorityHandoffAndMask`
- `VmNet1441SliceTests.LevelStartReleaseAndSeat`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1441 --to 1441` (0 hard / 0 soft).

## Seams

Disconnect, demo status, prediction clear, and the adaptive lead are callees. NoteMalformedGameplayPacket starts at line 5796.

## Credit rationale

UNCredited d_net.cpp 5457-5792 (+336). One contiguous span of whole functions. Blank lines inside it are non-behavior. The span does not cross credited code.
