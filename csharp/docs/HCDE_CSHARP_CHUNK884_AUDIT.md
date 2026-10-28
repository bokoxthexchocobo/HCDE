# Chunk 884 — `P_TestActivateLine` body and `P_PredictLine` (`p_spec.cpp` 243–399); header **219–242** remains on **437**.

## Scope

`P_TestActivateLine` body and `P_PredictLine` (`p_spec.cpp` 243–399); header **219–242** remains on **437**.

## Native ranges

- `src/playsim/p_spec.cpp` **P_TestActivateLine** 243–356 (114 lines)
- `src/playsim/p_spec.cpp` **P_PredictLine** 364–399 (36 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPSpecTestActivateLine.cs`, `VmPSpecPredictLine.cs`
Managed LOC (tally): 252.
Conversion commit: `764878f4548aa1a5faf1ffda2ea75f0044f57bb5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `P_TestActivateLine` **243–356** | **yes** (`VmPSpecTestActivateLine.TestActivateLine`) |
| `P_PredictLine` **364–399** | **yes** (`VmPSpecPredictLine.PredictLine`) |

| Managed | Native (`p_spec.cpp`) |
|---------|----------------------|
| `TestActivateLine` | **243–356** |
| `PredictLine` | **364–399** |

**Single-credit rule**

| Lines | Owner |
|-------|--------|
| **219–242** | **437** (header / pre-body) |
| **243–399** | **884** |
| **406–501** | **805** |
| **875** `b_move.cpp` | **no** `p_spec` credit |

### Padding map

| Native lines | Reason |
|--------------|--------|
| **357–363** | separators between functions (in hull, no extra symbols) |

### Gross vs net

| | Value |
|--|------:|
| Gross native (`nativeLoc`) | **157** |
| Net behavior-ported | **157** |

### Out-of-body uncredit (884)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **0** |
| **Honest** | **150** |


**Post-review ratio:** 252/150 managed/native ≈ 168% (src file LOC at HEAD; see tally).

## Tests

- `VmPSpecTestActivateLineTests.TestActivateLine_AllowsMonsterCrossTeleport`
- `VmPSpecTestActivateLineTests.PromoteMissileTeleport_WhenCrossTeleport`
- `VmPSpecPredictLineTests.PredictTeleport_WhenActivateAndCvarOk`
- `VmPSpecPredictLineTests.RejectWhenPredictCvarOff`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 884 --to 884` (0 hard / 0 soft).

## Seams

`P_ExecuteSpecial` predict path calls `LineSpecials.ExecuteTeleportSpecial` / `LineSpecialMisc` teleport-line directly; diag print and net trace omitted.

## Credit rationale

Contiguous uncredited band (**157** hull) adjacent to **437** header and **805**; re-credits existing `VmPSpecTestActivateLine` plus faithful `P_PredictLine` (no **438** row).
