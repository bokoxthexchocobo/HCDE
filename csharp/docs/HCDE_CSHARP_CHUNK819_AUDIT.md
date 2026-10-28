# Chunk 819 — `COPY_AAPTR` / `COPY_AAPTREX`, chain verification, and `ASSIGN_AAPTR` from `actorptrselect.cpp` (55–248).

## Scope

`COPY_AAPTR` / `COPY_AAPTREX`, chain verification, and `ASSIGN_AAPTR` from `actorptrselect.cpp` (55–248).

## Native ranges

- `src/playsim/actorptrselect.cpp` **COPY_AAPTREX** 55–138 (84 lines)
- `src/playsim/actorptrselect.cpp` **COPY_AAPTR** 142–146 (5 lines)
- `src/playsim/actorptrselect.cpp` **VerifyTargetChain** 162–193 (32 lines)
- `src/playsim/actorptrselect.cpp` **VerifyMasterChain** 195–219 (25 lines)
- `src/playsim/actorptrselect.cpp` **ASSIGN_AAPTR** 230–248 (19 lines)

**Credited native LOC:** 165 (tally `nativeLoc` 165).

## Managed

`VmActorPtrSelect.cs`
Managed LOC (tally): 208.
Conversion commit: `869c80eb20532cfc1df6803ce5f743097f952742`.

## Padding review (2026-10-21)

| Managed | Native (`actorptrselect.cpp`) |
|---------|-------------------------------|
| `CopyAaptrex` / `CopyAaptr` | **55–146** |
| `VerifyTargetChain` / `VerifyMasterChain` | **162–219** |
| `AssignAaptr` | **230–248** |

### Out-of-body uncredit (819)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **165** |
| **Out-of-body removed** | **0** |
| **Honest** | **165** |


**Post-review ratio:** 208/165 managed/native ≈ 126% (src file LOC at HEAD; see tally).

## Tests

- `VmActorPtrSelectTests.CopyAaptrex_DefaultReturnsOrigin`
- `VmActorPtrSelectTests.CopyAaptrex_TargetSelectorReturnsTarget`
- `VmActorPtrSelectTests.VerifyTargetChain_ClearsLoop`
- `VmActorPtrSelectTests.AssignAaptr_SetsTracerWithoutVerify`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 819 --to 819` (0 hard / 0 soft).

## Seams

`P_BulletSlope`, player in-game checks, and client-side barrier rules are injected via callbacks.

## Credit rationale

First tally credit for actor pointer selector resolution and target/master loop breaking.
