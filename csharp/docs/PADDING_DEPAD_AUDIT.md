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
