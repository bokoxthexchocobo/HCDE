# Chunk 1458 — Capability-gated live reject and adaptive bandwidth tick in d_net.cpp (1993-2251).

## Scope

Capability-gated live reject and adaptive bandwidth tick in d_net.cpp (1993-2251).

## Native ranges

- `src/d_net.cpp` **HCDERejectLiveGameplayForCapabilities** 1993–2251 (259 lines)

**Credited native LOC:** 259 (tally `nativeLoc` 259).

## Managed

`VmNet1458Slice.cs`
Managed LOC (tally): 260.
Conversion commit: `9988a49aff5b22b7545488411537132409b945e5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| Reject and bandwidth tick (**1993–2251**) | yes |
| Band hull (**1993–2251**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| Legacy gameplay reject | lines 1770-1985 are already credited |
| Bandwidth reset | starts at line 2253 |
| Bandwidth CVAR block | definitions stay later |

A capability reject logs at most once per second and folds the suppressed count into the next line. The auto tick demotes one step, or straight to the player-count cap when that cap is lower, and promotes only after a clean window outside the cooldown.

| Managed (`VmNet1458Slice.cs`) | Native (`d_net.cpp`) |
|-------------------------------|----------------------|
| Reject through Tick | **1993–2251** |
| Band hull (exclusive slice) | **1993–2251** |

### Chunk **1458** gross vs net

| | Value |
|--|------:|
| Row nativeLoc | **259** |
| **Net headline** | **+259** vs the prior chunk |


**Post-review ratio:** 260/259 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmNet1458SliceTests.CapabilityRejectThrottlesPerSecond`
- `VmNet1458SliceTests.BandwidthAutoDemotesPromotesAndBudgets`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1458 --to 1458` (0 hard / 0 soft).

## Seams

HCDERejectLiveGameplayForCapabilities through Net_TickHCDEBandwidthAuto. Legacy reject ends at 1985. Net_ResetHCDEBandwidthAutoState starts at 2253. The bandwidth CVAR definitions stay outside this span.

## Credit rationale

UNCredited d_net.cpp 1993-2251 (+259). One contiguous span of the throttled capability reject and the adaptive bandwidth tick. The span does not cross credited code.
