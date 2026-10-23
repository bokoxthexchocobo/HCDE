# Chunk 438 — Retired (2026-10-23): post-875 re-audit — `P_PredictLine` was predicate-only (behavior **no**); remaining `P_TestActivateLine` band **114** native &lt; **150** floor. Full `TestActivateLine` stays off-tally in `VmPSpecTestActivateLine` (875 calls it; no `p_spec.cpp` credit on **875**). Re-credit when a single chunk owns **≥150** contiguous honest `p_spec.cpp` activation lines.

## Scope

Retired (2026-10-23): post-875 re-audit — `P_PredictLine` was predicate-only (behavior **no**); remaining `P_TestActivateLine` band **114** native &lt; **150** floor. Full `TestActivateLine` stays off-tally in `VmPSpecTestActivateLine` (875 calls it; no `p_spec.cpp` credit on **875**). Re-credit when a single chunk owns **≥150** contiguous honest `p_spec.cpp` activation lines.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `df2610badfd9f9d9f1d288b19a5be8c3dde7874a`.

## Padding review (2026-10-21)

### Re-audit (post-875)

| Native (`p_spec.cpp`) | Behavior ported in chunk managed code | Matrix credit |
|-------------------------|----------------------------------------|---------------|
| `P_TestActivateLine` **243–356** (**114**) | **yes** in `VmPSpecTestActivateLine` (off-tally; below floor alone) | **no** (retired) |
| `P_PredictLine` **364–399** (**36**) | **no** — `VmPSpecPredictLine` predicates only; no `P_ExecuteSpecial` | **no** |
| Heal **478–495** (historical) | credited on **805** only | **no** |

### Gross vs net (**438**)

| | Value |
|--|------:|
| Gross native attempted | **150** |
| Net behavior-ported (matrix) | **0** |

### Padding map

| Native lines | Reason |
|--------------|--------|
| **243–356** | Honest port off-tally; chunk retired under **150** floor |
| **364–399** | Predicate stubs — not behavior-ported |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 438 --to 438` (0 hard / 0 soft).

## Seams

Off-tally `VmPSpecTestActivateLine` / `VmPSpecPredictLine` (predicate stubs only for predict). `VmBBotMove` (875) calls `TestActivateLine` on spechit lines only.

## Credit rationale

Matrix credit must not duplicate **805** `P_ActorInSpecialSector` (**406–501**) or claim `P_PredictLine` without `P_ExecuteSpecial` / predict execution path.
