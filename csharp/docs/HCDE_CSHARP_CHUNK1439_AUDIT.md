# Chunk 1439 — Echo string read through local echo compare in d_net_diag.cpp (43-316).

## Scope

Echo string read through local echo compare in d_net_diag.cpp (43-316).

## Native ranges

- `src/d_net_diag.cpp` **HCDEReadEchoString** 43–316 (274 lines)

**Credited native LOC:** 274 (tally `nativeLoc` 274).

## Managed

`VmNetDiag1439Slice.cs`
Managed LOC (tally): 225.
Conversion commit: `08a453cb6439c72ade78adf034d7282af0cda607`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Echo string read, weapon-change flags, follow, ping, and compare (**43–316**) | yes |
| Band hull (**43–316**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Replicated-item struct | lines 319-326 stay outside this span |
| Append echo string | lines 32-41 stay outside this span |
| CVAR, static slots, and comments | non-behavior inside **43–316** |

A length is big-endian. Zero copies nothing. A length above 255 is rejected after the length bytes are consumed. A short payload is rejected the same way. The first echo for a client reports a ready-class change and a forced reseat. A later ready-class change also forces a reseat. An owner that has just caught the ready weapon, a lost ready bit, or an owner change with a new state offset forces a reseat on its own. A client outside 0 through 63 is ignored. Follow runs only for the console player, only when the cvar is on, the peer is not the authority, and the change flags are nonzero. A missing class or inventory item returns before the weapon state changes. A subclass inventory item matches. A fixed sprite keeps the frame and sprite. A no-change sprite keeps the sprite. The same-frame flag keeps the frame. Compare walks to the weapon layer, logs class, state, weapon, and player mismatches, and logs view height only for the console player when the difference is greater than one.

| Managed (`VmNetDiag1439Slice.cs`) | Native (`d_net_diag.cpp`) |
|-----------------------------------|---------------------------|
| Echo read through local compare | **43–316** |
| Band hull (exclusive slice) | **43–316** |

### Chunk **1439** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **274** |
| **Net headline** | **+274** vs the prior chunk |


**Post-review ratio:** 225/274 managed/native ≈ 82% (src file LOC at HEAD; see tally).

## Tests

- `VmNetDiag1439SliceTests.ReadFlagsAndRemember`
- `VmNetDiag1439SliceTests.FollowReseatCompareAndPing`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1439 --to 1439` (0 hard / 0 soft).

## Seams

BE16 read, actor lookup, and debug trace are callees. The replicated-item struct starts at line 319.

## Credit rationale

UNCredited d_net_diag.cpp 43-316 (+274). One contiguous span. The CVAR, static slots, and comments inside it are non-behavior. The span does not cross credited code.
