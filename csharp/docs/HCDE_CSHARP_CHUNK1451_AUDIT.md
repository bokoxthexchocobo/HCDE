# Chunk 1451 — Actor-delta codecs and invasion snapshot header in d_net.cpp (3071-3349).

## Scope

Actor-delta codecs and invasion snapshot header in d_net.cpp (3071-3349).

## Native ranges

- `src/d_net.cpp` **HCDEQuantizeActorDeltaPos** 3071–3349 (279 lines)

**Credited native LOC:** 279 (tally `nativeLoc` 279).

## Managed

`VmNet1451Slice.cs`
Managed LOC (tally): 371.
Conversion commit: `bdb29a050c9e22267970461532ae99fc6099ccd8`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Deltas and invasion header (**3071–3349**) | yes |
| Band hull (**3071–3349**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Field codecs | lines 2814-3069 are already credited |
| Snapshot include body | the reads after roll live in the included fragment |
| Apply invasion snapshot | starts at line 3351 |

Position quantizing rounds half away from zero and clamps to 32 bits. A still pawn drops look-only view error. The invasion header is 52 bytes, and a lane-budget cap clamps the following event and actor-delta sections.

| Managed (`VmNet1451Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| QuantizePos through AppendInvasion | **3071–3349** |
| Band hull (exclusive slice) | **3071–3349** |

### Chunk **1451** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **279** |
| **Net headline** | **+279** vs the prior chunk |


**Post-review ratio:** 371/279 managed/native ≈ 133% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1451SliceTests.QuantizeBasisViewAndCommand`
- `VmNet1451SliceTests.InvasionHeaderAndLaneBudget`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1451 --to 1451` (0 hard / 0 soft).

## Seams

HCDEQuantizeActorDeltaPos through HCDEAppendInvasionSnapshot. HCDEReadFloatField ends at 3069. HCDEApplyInvasionSnapshot starts at 3351.

## Credit rationale

UNCredited d_net.cpp 3071-3349 (+279). One contiguous span of the delta codecs, the view-error decay, and the invasion snapshot header. The included snapshot fragment is stepped over. The span does not cross credited code.
