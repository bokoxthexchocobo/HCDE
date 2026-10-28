# Chunk 694 — Whole `P_TouchSpecialThing`, `PronounMessage`, and `ClientObituary` (`p_interaction.cpp` 85–108, 123–181, 186–321).

## Scope

Whole `P_TouchSpecialThing`, `PronounMessage`, and `ClientObituary` (`p_interaction.cpp` 85–108, 123–181, 186–321).

## Native ranges

- `src/playsim/p_interaction.cpp` **P_TouchSpecialThing** 85–108 (24 lines)
- `src/playsim/p_interaction.cpp` **PronounMessage** 123–181 (59 lines)
- `src/playsim/p_interaction.cpp` **ClientObituary** 186–321 (136 lines)

**Credited native LOC:** 219 (tally `nativeLoc` 219).

## Managed

`VmPInteractionTouchObituary.cs`
Managed LOC (tally): 268.
Conversion commit: `c6dc96c4a7b10b4810dae72620791f6c2cd522f1`.

## Padding review (2026-10-21)

| Managed | Native (`p_interaction.cpp`) |
|---------|---------------------------|
| `VmPInteractionTouchObituary` | `P_TouchSpecialThing` 85–108; `PronounMessage` 123–181; `ClientObituary` 186–321 |

### Out-of-body uncredit (694)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **219** |
| **Out-of-body removed** | **0** |
| **Honest** | **219** |


**Post-review ratio:** 268/219 managed/native ≈ 122% (src file LOC at HEAD; see tally).

## Tests

- `VmPInteractionTouchObituaryTests.TouchSpecialThing_CallsTouchWhenInReach`
- `VmPInteractionTouchObituaryTests.TouchSpecialThing_SkipsWhenOutOfReach`
- `VmPInteractionTouchObituaryTests.PronounMessage_ExpandsSubjectPronoun`
- `VmPInteractionTouchObituaryTests.ClientObituary_PrintsResolvedSuicide`
- `VmPInteractionTouchObituaryTests.ClientObituary_NoSkyWhenShowObituariesOff`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 694 --to 694` (0 hard / 0 soft).

## Seams

`CallTouch`, `GStrings`, `AnnounceKill`/`AnnounceTelefrag`, VM obituary hooks, `Printf`, and kill-feed push are injected.

## Credit rationale

Re-credits touch/obituary bands retired from chunk458 helpers with full-function managed bodies.
