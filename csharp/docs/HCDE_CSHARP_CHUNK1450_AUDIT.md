# Chunk 1450 — Stale-room recovery and field codecs in d_net.cpp (2814-3069).

## Scope

Stale-room recovery and field codecs in d_net.cpp (2814-3069).

## Native ranges

- `src/d_net.cpp` **UnwrapHCDEGameplayEnvelope** 2814–3069 (256 lines)

**Credited native LOC:** 256 (tally `nativeLoc` 256).

## Managed

`VmNet1450Slice.cs`
Managed LOC (tally): 244.
Conversion commit: `faadaf9ac26252c6e003d710fc40f637ed843b04`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Recovery and codecs (**2814–3069**) | yes |
| Band hull (**2814–3069**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Envelope prefix | lines 2628-2807 are already credited |
| Room staleness gate | lines 2809-2813 are already credited |
| Actor delta quantize | starts at line 3071 |

A non-authority client forces the cutscene to end when the authority room is exactly one ahead, and waits 300 ms before doing it again. A matching envelope exposes the payload after the 12-byte gameplay header. Non-finite doubles and floats are refused, and a non-finite read still consumes its bytes.

| Managed (`VmNet1450Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| RecoverStaleRoom through ReadFloat | **2814–3069** |
| Band hull (exclusive slice) | **2814–3069** |

### Chunk **1450** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **256** |
| **Net headline** | **+256** vs the prior chunk |


**Post-review ratio:** 244/256 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1450SliceTests.StaleRoomRecoveryAndTail`
- `VmNet1450SliceTests.AppendAndReadRoundTrip`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1450 --to 1450` (0 hard / 0 soft).

## Seams

Cutscene recovery inside the stale-room branch through HCDEReadFloatField. The room check at 2809-2813 is already credited. HCDEQuantizeActorDeltaPos starts at 3071.

## Credit rationale

UNCredited d_net.cpp 2814-3069 (+256). One contiguous span of the cutscene recovery and the append/read codecs. The span does not cross credited code.
