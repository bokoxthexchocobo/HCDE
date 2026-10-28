# Chunk 886 — Retired (2026-10-23): rejected **838-pattern** land — **156** cherry-picked “decision” lines in hull **1386–1841** (**456** lines) with banned seams (`C_MidPrint`, `Printf`, `Spawn`, `P_SpawnParticle`, `P_RemoveThing`, `Thrust`). Whole-function port with direct effects not done. Matrix credit **0**.

## Scope

Retired (2026-10-23): rejected **838-pattern** land — **156** cherry-picked “decision” lines in hull **1386–1841** (**456** lines) with banned seams (`C_MidPrint`, `Printf`, `Spawn`, `P_SpawnParticle`, `P_RemoveThing`, `Thrust`). Whole-function port with direct effects not done. Matrix credit **0**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `92bb9cbc494d72698c5940e3fb1eae63bb8e45ab`.

## Padding review (2026-10-21)

### Incident — first land attempts (policy, not flaky tests)

| Step | Outcome |
|------|--------|
| `land_chunk_fast.sh 886` (commit **4ebe6c9b**) | Stopped after `generate_native_managed_matrix` — **`reaudit_chunks.py`** hard fail **`audit_map_coverage_below_floor`** (~**0.55**): credited **165** lines vs audit-map **125** (cherry-picked nativeRanges vs padding table). **No tally commit, no verify, no push.** |
| `land_chunk_fast.sh --resume 886` (**b4e05906**) | Same gate after range trim — script exit **1** before tally (agent then **bypassed land gate** by hand-running `write_chunk_audit.py`, `git commit`, `fix_chunk_tally.py 576`, `verify_full_solution_tests.sh`) — **invalid**; superseded by this retire land. |
| `HCDE.MigrationMatrix.Tests` | **Not** the `land_chunk_fast` blocker on those runs (verify never reached). One **manual** verify saw **1** fail / **10** pass then **11/11** on immediate retry — treated as flake; **retire land must pass full verify inside script.** |

### **576** vs **886** gross/net (no double credit)

| | Chunk **576** | Chunk **886** (rejected land) | Chunk **886** (retired) |
|--|--------------:|------------------------------:|------------------------:|
| `p_actionfunctions` native credited | **1960+** (range/inventory/wolf/warp/user-field bands only) | **1386–1841** fragments (**156** `nativeLoc`) | **0** |
| Overlap with **576** `nativeRanges` | — | **none** (576 has no line **&lt;1960** in that file) | **none** |
| `managedSourceFiles` | Removed **Print/Fade/Sight** briefly (manifest-only); restored on retire commit | claimed those files | **[]** (matrix) |
| Gross matrix native | **268** (unchanged by **886** retire) | +**156** (reverted by retire) | **0** |
| Net behavior-ported | per **576** audit (decision+seam policy on **576** rows) | **0** (seam pattern) | **0** |

### Behavior ported (matrix credit)

| Native | Behavior ported |
|--------|-----------------|
| (none) | **no** |

### Padding map (rejected approach — not credited)

| Pattern | Verdict |
|---------|--------|
| Line-picked `A_Print` / fade / particle “setup” without `C_MidPrint` / `P_SpawnParticle` / `Spawn` | **838-pattern** — banned |
| Hull **1386–1841** with **156** credited fragments | **cherry-pick** — banned |

### Gross vs net (**886** retire)

| | Value |
|--|------:|
| Gross attempted (invalid land) | **156** |
| Net behavior-ported (matrix) | **0** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 886 --to 886` (0 hard / 0 soft).

## Seams

Prior attempt listed console/spawn/remove as seams; standing policy bans seams and partial-function line credit. Off-tally helpers remain in `VmActionPrintLogRecoil` / `VmActionFadeSpawn` / `VmActionSightParticle` (no matrix native credit).

## Credit rationale

User rejected landed **886** as **838-pattern**: decision logic credited while native effects stay unported. Acceptable fix paths were (a) contiguous **150–400** whole functions **1386–1841** with every effect a direct managed call, or (b) retire to **0** — **(b)**. Bot `ThinkForMove` still deferred (unchanged). **576 overlap:** chunk **576** never credited `p_actionfunctions.cpp` **1386–1841** in `nativeRanges` (lowest credited band starts **1960** `CheckRange`); **886** briefly moved three files off **576** `managedSourceFiles` only — manifest listing, not native double-count.
