# Chunk 731 — Whole `CallStateChain` and `AActor::GetZAt` (`p_actionfunctions.cpp` 87–229, 248–326). Retired invalid 87–790 oversize row.

## Scope

Whole `CallStateChain` and `AActor::GetZAt` (`p_actionfunctions.cpp` 87–229, 248–326). Retired invalid 87–790 oversize row.

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **CallStateChain** 87–229 (143 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::GetZAt** 248–326 (79 lines)

**Credited native LOC:** 222 (tally `nativeLoc` 222).

## Managed

`VmPActionCallStateChain.cs`
Managed LOC (tally): 170.
Conversion commit: `8d99202fa338184ab2e1685931aef6506dc2079d`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `CallStateChain` | `CallStateChain` 87–221; `DEFINE_ACTION_FUNCTION_NATIVE` 223–229 |
| `BuildReturnPlan` | return-type dispatch 135–153 |
| `GetZAt` | `DEFINE_ACTION_FUNCTION(AActor, GetZAt)` 248–326 |
| `GetZAtAbsolutePos` | relative position 275–280 |
| `SelectFloorZ` / `SelectCeilingZ` | floor/ceiling branches 305–319 |


**Post-review ratio:** 170/222 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmPActionCallStateChainTests.CallStateChain_SucceedsWhenActionReturnsTrue`
- `VmPActionCallStateChainTests.CallStateChain_MissingItemFlag_Fails`
- `VmPActionCallStateChainTests.CallStateChain_SelfJump_Aborts`
- `VmPActionCallStateChainTests.GetZAt_FloorNo3dNoPortal_UsesPlane`
- `VmPActionCallStateChainTests.GetZAt_NullMobj_ReturnsZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 731 --to 731` (0 hard / 0 soft).

## Seams

`VMCallAction`, `COPY_AAPTR`, sector plane queries, and portal iterators are injected.

## Credit rationale

Two whole functions in one playsim TU: inventory state-chain dispatch and GetZAt floor/ceiling sampling.
