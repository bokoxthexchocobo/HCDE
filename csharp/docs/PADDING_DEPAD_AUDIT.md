# Padding depad audit (post-restore purge)

Strict `reaudit_chunks.py padding-scan` (no gate narrowing). Methods matching constant/identity/empty shapes are removed per chunk policy:

| Action | When |
|--------|------|
| **Delete** | Zero call sites (excluding the definition) |
| **Inline + delete** | Called identity/constant wrapper; expression inlined at qualified/unqualified call sites |
| **auditPaddingReview** | Rare: distinct native function with trivial surface shape |

Tool: `csharp/tools/depad_padding_methods.py`

## Batch 1 — `VmPMap*` (Playsim)

- **Deleted:** 154 uncalled wrappers
- **Inlined:** 16 called identity/constant wrappers (incl. tests)
- **Justified:** 0
- **padding-scan 450–647:** 718 → 549
- **Verified:** `dotnet build HCDE.sln -warnaserror`; `dotnet test HCDE.sln`

## Batch 2 — remaining Playsim (non-`VmPMap`)

- **Deleted:** 415 uncalled wrappers
- **Inlined:** 61 called wrappers (+ manual chain fixes on `PlayerMoFreeze` / `IncrementKilledMonsters`)
- **Justified:** 0
- **padding-scan 450–647:** 549 → 74
- **Verified:** `dotnet build HCDE.sln -warnaserror`; `dotnet test HCDE.sln`

## Batch 3 — Scripting + Net.Core (committed session 20M)

- **Deleted:** 56
- **Inlined:** 16 (+ manual `MarkUnsafeContext` test inline)
- **Justified:** 0
- **padding-scan 450–647:** 74 → 0 (`reaudit_chunks` skips control-flow `if {}` false positives)
- **Verified:** `dotnet build HCDE.sln -warnaserror`; `dotnet test HCDE.sln`; MigrationMatrix 8/8

## Totals (batches 1–3)

| | Count |
|--|--:|
| Deleted (uncalled) | 625 |
| Inlined | 93 |
| auditPaddingReview justified | 0 |
