# Chunk 831 — `DPlat` motion/sound helpers, `Tick`, `Reactivate`, full `EV_DoPlat`, and `ActivateInStasisPlat` from `mapthinkers/a_plats.cpp` (76–468).

## Scope

`DPlat` motion/sound helpers, `Tick`, `Reactivate`, full `EV_DoPlat`, and `ActivateInStasisPlat` from `mapthinkers/a_plats.cpp` (76–468).

## Native ranges

- `src/playsim/mapthinkers/a_plats.cpp` **DPlat::PlayPlatSound** 76–92 (17 lines)
- `src/playsim/mapthinkers/a_plats.cpp` **DPlat::GetSoundByType** 94–97 (4 lines)
- `src/playsim/mapthinkers/a_plats.cpp` **DPlat::Tick** 105–227 (123 lines)
- `src/playsim/mapthinkers/a_plats.cpp` **DPlat::Reactivate** 235–241 (7 lines)
- `src/playsim/mapthinkers/a_plats.cpp` **FLevelLocals::EV_DoPlat** 274–457 (184 lines)
- `src/playsim/mapthinkers/a_plats.cpp` **FLevelLocals::ActivateInStasisPlat** 458–468 (11 lines)

**Credited native LOC:** 346 (tally `nativeLoc` 346).

## Headline gross vs net

| Anchor | `convertedBehaviorNative` |
| --- | ---: |
| After chunk **830** tally (`7aa92e8b`) | **97,464** |
| After chunk **831** tally (`73f247e3`) | **97,810** |
| **Net** | **+346** (= row gross **346**) |

## Portfolio delta

**Gross CLI band:** **393** lines in the UNCredited-range probe; tally credits **346** whole-function native LOC only.
**47** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **393** |
| **Credited** (whole functions, this row) | **346** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+346** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmAPlats.cs`
Managed LOC (tally): 343.
Conversion commit: `770ed362f4dbac2455537b0b5fea066872383201`.

## Padding review (2026-10-21)

| Managed | Native (`a_plats.cpp`) |
|---------|----------------------|
| `PlayPlatSound` | **76–92** |
| `GetSoundByType` | **94–97** |
| `Tick` | **105–227** |
| `Reactivate` | **235–241** |
| `EvDoPlat` | **274–457** |
| `ActivateInStasisPlat` | **458–468** |

**Deferred (uncredited, &lt;150 LOC combined):** `Serialize` **44–57**, `Stop` **249–253**, `Construct` **261–264**, `EV_StopPlat` **476–492**.

**Post-review ratio:** 343/346 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmAPlatsTests.Tick_UpPastDest_RaiseAndStayDestroys`
- `VmAPlatsTests.GetSoundByType_Stone_ReturnsFloor`
- `VmAPlatsTests.Tick_Waiting_CountdownFlipsToUp`
- `VmAPlatsTests.Reactivate_Toggle_SwapsDirection`
- `VmAPlatsTests.EvDoPlat_RaiseAndStay_SetsUpStatus`
- `VmAPlatsTests.EvDoPlat_Toggle_ActivatesInStasis`
- `VmAPlatsTests.ActivateInStasisPlat_ReactivatesMatchingTag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 831 --to 831` (0 hard / 0 soft).

## Seams

Sector floor moves, height queries, tag iteration, thinker factory, and sound sequences are injected; no `DMovingFloor` base VM.

## Credit rationale

First tally credit for plat thinker tick and line-special plat dispatch in `a_plats.cpp` (whole `EV_DoPlat` 274–457).
