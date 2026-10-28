# Chunk 881 — Deferred (2026-10-23): `p_secnodes.cpp` **284–475** passes `uncredited_range_check` as one band (**192** lines) but native `AActor::UpdateRenderSectorList` / `touching_*portallist` have no managed `Actor` fields or playsim tick caller yet — port would be test-only (same policy as deleted **880** leftovers).

## Scope

Deferred (2026-10-23): `p_secnodes.cpp` **284–475** passes `uncredited_range_check` as one band (**192** lines) but native `AActor::UpdateRenderSectorList` / `touching_*portallist` have no managed `Actor` fields or playsim tick caller yet — port would be test-only (same policy as deleted **880** leftovers).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Portfolio delta

**Gross CLI band:** **192** lines in the UNCredited-range probe; tally credits **0** whole-function native LOC only.
**192** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **192** |
| **Credited** (whole functions, this row) | **0** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+0** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `663ecc0897e569eb1f798fb6acb579627cad6a2a`.

## Padding review (2026-10-21)

### vs chunk **810** (no double count)

| | Lines |
|--|------:|
| **810** credited (`p-secnodes-freelist-chunk810`) | **242** (**42–283**) |
| **881** gross band (deferred) | **192** (**284–475**) |
| **881** net matrix credit | **0** |

**810 credited symbols (unchanged):** `P_GetSecnode` / `P_PutSecnode`, `P_AddSecnode`, `P_DelSecnode`, `P_DelSeclist`, `P_CreateSecNodeList`.

**881 deferred native (function boundaries):**

| Native | Lines |
|--------|------:|
| `P_DelPortalnode` | **293–329** |
| `P_AddPortalnode` | **338–367** |
| `AActor::UpdateRenderSectorList` | **375–427** |
| `AActor::ClearRenderSectorList` / `ClearRenderLineList` | **429–439** |
| `FBlockNode::Create` / `Release` | **449–475** |

### Deferred-bands registry

| File | Band | Lines | Blocker |
|------|------|------:|---------|
| `p_linkedsectors.cpp` | see chunk **880** | 225 (non-contiguous) | no ≥150 contiguous band |
| `p_secnodes.cpp` | **284–475** | **192** | no actor render-portal playsim wiring |

### Behavior ported (matrix credit)

| Native | Behavior ported |
|--------|-----------------|
| (none) | **no** (deferred) |

### Gross vs net

| | Value |
|--|------:|
| Gross band surveyed | **192** |
| Net behavior-ported | **0** |

## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 881 --to 881` (0 hard / 0 soft).

## Seams

Chunk **810** credited **42–283**; `VmPSecnodes` is used from `VmPUserHcdeWeaponPhysics` (sector thing lists). Portal `portnode_t` / `FBlockNode` / render lists require `OldRenderPos`, `Level.linePortals`, and `PortalBlockmap` on the actor tick path before honest credit.

## Credit rationale

Band is contiguous and ≥150 native lines, but landing without wiring `UpdateRenderSectorList` into playsim would duplicate **878**/**880** test-only off-tally code.
