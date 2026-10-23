# Chunk 874 — Retired (2026-10-23): whole TU `a_specialspot.cpp` **1–305** had faithful list/state ports in `VmASpecialSpot.cs`, but **~150** surviving managed lines vs **153** minimum (**50%** of **305** native gross) after removing coverage-padding `NewSpotList`. Band stays uncredited (same deferral policy as **826** / **834**).

## Scope

Retired (2026-10-23): whole TU `a_specialspot.cpp` **1–305** had faithful list/state ports in `VmASpecialSpot.cs`, but **~150** surviving managed lines vs **153** minimum (**50%** of **305** native gross) after removing coverage-padding `NewSpotList`. Band stays uncredited (same deferral policy as **826** / **834**).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

`VmASpecialSpot.cs`, `VmASpecialSpotTests.cs`
Managed LOC (tally): 0.
Conversion commit: `4c5d59e61cb99315fb1f67102577efd2c12d0633`.

## Padding review (2026-10-21)

### Padding incident (chunk **874**)

| Artifact | Native basis | Verdict |
|----------|--------------|--------|
| `NewSpotList` | `FSpotList(PClassActor*)` ctor **53–59** (inline in `FindSpotList` **227**) | **Padding** — extracted helper to reach **154** managed LOC; banned top-up |

### Behavior ported (off-tally, deleted — not matrix-credited)

| Managed | Native (`a_specialspot.cpp`) |
|---------|---------------------------|
| `SpotList.Add` | **67–75** |
| `SpotList.Remove` | **83–95** |
| `SpotList.GetNextInList` | **103–115** |
| `SpotList.GetSpotWithMinMaxDistance` | **123–142** |
| `SpotList.GetRandomSpot` | **150–159** |
| `SerializeSpotList` / `DeserializeSpotList` | **168–180** (field mirror; no `FSerializer`) |
| `SpotState.OnDestroy` | **198–202** |
| `SpotState.FindSpotList` | **220–228** |
| `SpotState.AddSpot` | **236–241** |
| `SpotState.RemoveSpot` | **249–254** |
| `SpotState.GetNextInList` | **274–279** |
| `SpotState.GetSpotWithMinMaxDistance` | **287–292** |
| `SpotState.GetRandomSpot` | **300–305** |
| `Distance2D` | distance loop **133** / `AActor::Distance2D` usage |

**Not ported (zero credit)**

| Native | Policy |
|--------|--------|
| `DSpotState::Serialize` (**262–266**) | `FSerializer` host deferred |
| IMPLEMENT / headers / empty ctor-tick (**1–66**, **188–212**, separators) | Gross envelope only |

### Gross vs net

| | Value |
|--|------:|
| Gross native envelope attempted | **305** |
| Prior active `nativeLoc` (tally **7c569a75**) | **305** |
| Surviving managed (honest, no `NewSpotList`) | **~150** |
| `check_port_coverage` minimum (**50%**) | **153** |
| Matrix credit after retire | **0** |

## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 874 --to 874` (0 hard / 0 soft).

## Seams

Deleted managed port and tests (2026-10-23). `pr_spot` RNG remains injected at call sites in any future honest re-land; no `FSerializer` host for save-game paths.

## Credit rationale

Padding incident: commit **c22f0dbe** added `NewSpotList` solely to pass `check_port_coverage.sh` after **8a33e40d** dropped empty `Tick`; without that helper, `managed_src_head=150` &lt; **153** need. Whole-file gross credit **305** includes headers/serialize/separators that have no additional faithful managed bodies — honest behavior map tops out below the ratio floor. No overlap with prior tally rows (**826** never landed; **778**/**782** removed mistaken `a_specialspot` credit per **ee29666e**).
