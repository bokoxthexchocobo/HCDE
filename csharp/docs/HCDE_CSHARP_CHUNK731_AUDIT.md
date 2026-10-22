# Chunk 731 — Whole static `CallStateChain` band through `A_SeekerMissile` (`p_actionfunctions.cpp` 87–790).

## Scope

Whole static `CallStateChain` band through `A_SeekerMissile` (`p_actionfunctions.cpp` 87–790).

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **CallStateChain** 87–790 (704 lines)

**Credited native LOC:** 704 (tally `nativeLoc` 704).

## Managed

`VmPActionCallStateChain.cs`
Managed LOC (tally): 384.
Conversion commit: `a9a9e6f3240e2ab771a2eaa7c31dd9355fcbddca`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `VmPActionCallStateChain` (aggregate band) | 87–790 |
| `CallStateChain` | `CallStateChain` 87–221 |
| `BuildReturnPlan` | return-type dispatch 135–153 |
| `GetZAtFloor` | `AActor::GetZAt` floor branch 305–318 |
| `GetCrouchFactor` | `AActor::GetCrouchFactor` 336–355 |
| `DecorateInternalInt` / `DecorateInternalBool` / `DecorateInternalFloat` | `__decorate_internal_*` 494–518 |
| `SeekerMissileShouldStick` | `A_SeekerMissile` sticky tracer 755–761 |


**Post-review ratio:** 384/704 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmPActionCallStateChainTests.CallStateChain_SucceedsWhenActionReturnsTrue`
- `VmPActionCallStateChainTests.CallStateChain_MissingItemFlag_Fails`
- `VmPActionCallStateChainTests.CallStateChain_SelfJump_Aborts`
- `VmPActionCallStateChainTests.BuildReturnPlan_StateAndInt`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 731 --to 731` (0 hard / 0 soft).

## Seams

`VMCallAction`, `P_RoughMonsterSearch`, `P_SeekerMissile`, `S_Sound`, and `COPY_AAPTR` are injected.

## Credit rationale

Single-function oversize exception (~704 native LOC): inventory state chain VM dispatch plus adjacent non-action helpers in the credited band.
