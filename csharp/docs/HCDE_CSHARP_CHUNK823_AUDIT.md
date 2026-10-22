# Chunk 823 — `FDynamicLight` release, activate, tick, and location update from `a_dynlight.cpp` (196–417).

## Scope

`FDynamicLight` release, activate, tick, and location update from `a_dynlight.cpp` (196–417).

## Native ranges

- `src/playsim/a_dynlight.cpp` **FDynamicLight::ReleaseLight** 196–205 (10 lines)
- `src/playsim/a_dynlight.cpp` **FDynamicLight::Activate** 213–231 (19 lines)
- `src/playsim/a_dynlight.cpp` **FDynamicLight::Tick** 239–352 (114 lines)
- `src/playsim/a_dynlight.cpp` **FDynamicLight::UpdateLocation** 362–417 (56 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

### Sub-band without `UpdateLocation`

| Functions | Lines | `nativeLoc` |
| --- | --- | ---: |
| `ReleaseLight` + `Activate` + `Tick` only | 196–205, 213–231, 239–352 | **143** |
| `UpdateLocation` (added band) | 362–417 | **56** |

### Why `UpdateLocation` is in this row (not a 150-floor pad)

Native **`FDynamicLight::Tick`** ends every successful path with a direct call to **`UpdateLocation()`** (`a_dynlight.cpp` **351**). `UpdateLocation` is the next whole function in the same TU (**362–417**), uncredited before this chunk, and implements the sector/light-node relink that makes tick radius changes visible. It was credited because it is part of the native tick contract, not because **143** was under the 150 minimum.

## Headline gross vs net

| Anchor | `convertedBehaviorNative` |
| --- | ---: |
| After chunk **822** tally (`2a427ee8`) | **96,129** |
| After chunk **823** tally (`06c91f34`) | **96,328** |
| **Net** | **+199** (= row gross **199**) |

## Managed

`VmADynLightTick.cs`
Managed LOC (tally): 158.
Conversion commit: `91b8f72bc7cc0ae58f63c8f308bbf700854399ca`.

## Padding review

| Managed | Native (`a_dynlight.cpp`) |
|---------|---------------------------|
| `ReleaseLight` | **196–205** |
| `Activate` | **213–231** |
| `Tick` | **239–352** |
| `UpdateLocation` | **362–417** |

**Post-review ratio:** 158/199 managed/native ≈ 79% (src file LOC at HEAD; see tally).

## Tests

- `VmADynLightTickTests.ReleaseLight_InvokesPoolReturn`
- `VmADynLightTickTests.Activate_PointLightSetsRadius`
- `VmADynLightTickTests.Tick_NoTarget_Releases`
- `VmADynLightTickTests.Tick_FlickerLight_PicksIntensity`
- `VmADynLightTickTests.UpdateLocation_RelinksWhenActive`

## How this chunk reached `origin` (push path)

| Step | What happened |
| --- | --- |
| 1 | `land_chunk_fast.sh 823` on **detached HEAD** (leftover from hook-proof / verify checkouts): port **`91b8f72b`**, tally **`06c91f34`**, in-script verify **passed**, **`git push` failed** (`fatal: invalid refspec ''` — no branch name). |
| 2 | **Not** `--resume`. **Not** hand `HCDE_ALLOW_PUSH=1 git push` for the tally. |
| 3 | `git checkout cursor/native-managed-matrix-3df6` + **fast-forward merge** `06c91f34`, then **`land_chunk_fast.sh consolidate`** commit **`9722d1d6`** (land script guard + push) → **`origin`**. |

### Hardened verify at tally SHA **`06c91f34`** (exact tree)

```
verify_full_solution_tests: all 14 projects passed (0 failed); build_errors=0
OK   HCDE.Playsim.Tests.dll passed=18934 failed=0 total=18934 (floor 18700)
```

Full log: `/opt/cursor/artifacts/verify_at_chunk823_tally_06c91f34.log`.

## Verified (HEAD)

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 823 --to 823` (0 hard / 0 soft).

## Seams

Level light lists, `FCycler`, sector light levels, and actor visibility are injected.

## Credit rationale

First tally credit for dynamic light lifetime and per-tic radius animation including the tick tail call to `UpdateLocation`.
