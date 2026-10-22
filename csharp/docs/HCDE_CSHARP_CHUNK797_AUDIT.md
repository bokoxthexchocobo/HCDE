# Chunk 797 — `DCeiling::PlayCeilingSound`, `Tick`, and ceiling EV helpers (`mapthinkers/a_ceiling.cpp` 97–117, 125–215, 506–621).

## Scope

`DCeiling::PlayCeilingSound`, `Tick`, and ceiling EV helpers (`mapthinkers/a_ceiling.cpp` 97–117, 125–215, 506–621).

## Native ranges

- `src/playsim/mapthinkers/a_ceiling.cpp` **DCeiling::PlayCeilingSound** 97–117 (21 lines)
- `src/playsim/mapthinkers/a_ceiling.cpp` **DCeiling::Tick** 125–215 (91 lines)
- `src/playsim/mapthinkers/a_ceiling.cpp` **FLevelLocals::EV_DoCeiling** 506–551 (46 lines)
- `src/playsim/mapthinkers/a_ceiling.cpp` **FLevelLocals::ActivateInStasisCeiling** 552–577 (26 lines)
- `src/playsim/mapthinkers/a_ceiling.cpp` **FLevelLocals::EV_CeilingCrushStop** 578–606 (29 lines)
- `src/playsim/mapthinkers/a_ceiling.cpp` **FLevelLocals::EV_StopCeiling** 607–621 (15 lines)

**Credited native LOC:** 228 (tally `nativeLoc` 228).

## Managed

`VmACeilingTickEv.cs`
Managed LOC (tally): 265.
Conversion commit: `360896ad9611d8897e56623a37a4f4cbdb4230b8`.

## Padding review (2026-10-21)

| Managed | Native (`a_ceiling.cpp`) |
|---------|-------------------------|
| `PlayCeilingSound` / `PlayCeilingSoundSector` | `DCeiling::PlayCeilingSound` **97–117** |
| `Tick` | `DCeiling::Tick` **125–215** |
| `EvDoCeiling` | `FLevelLocals::EV_DoCeiling` **506–551** |
| `ActivateInStasisCeiling` | `FLevelLocals::ActivateInStasisCeiling` **552–577** |
| `EvCeilingCrushStop` | `FLevelLocals::EV_CeilingCrushStop` **578–606** |
| `EvStopCeiling` | `FLevelLocals::EV_StopCeiling` **607–621** |


**Post-review ratio:** 265/228 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmACeilingTickEvTests.PlayCeilingSound_DefaultPicksCeilingNormal`
- `VmACeilingTickEvTests.Tick_UpPastDest_DestroyThinker`
- `VmACeilingTickEvTests.Tick_DownCrushSlowdown_ReducesSpeed`
- `VmACeilingTickEvTests.EvDoCeiling_ManualTag_CallsCreate`
- `VmACeilingTickEvTests.ActivateInStasisCeiling_RestoresDirection`
- `VmACeilingTickEvTests.EvCeilingCrushStop_PutsCrusherInStasis`
- `VmACeilingTickEvTests.EvStopCeiling_DestroyThinkerOnTaggedSector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 797 --to 797` (0 hard / 0 soft).

## Seams

`MoveCeiling`, sequence helpers, thinker iterators, and tag iterators are injected on tick/EV contexts.

## Credit rationale

Ceiling runtime band paired with `CreateCeiling` chunk796.

## Manual review (2026-10-22)

- **Non-contiguous bands** are six **whole** functions; gaps are uncredited material only:
  - **66–85** `DEFINE_FIELD` / `DEFINE_ACTION_FUNCTION` getters — not credited
  - **223–249** `DCeiling::Construct` overloads — credited on chunk **796** scope excluded here
  - **481–505** `DEFINE_ACTION_FUNCTION(FLevelLocals, CreateCeiling)` — not credited
- **`PlayCeilingSoundSector`:** Thin overload splitting sector vs thinker for tests; same logic as **97–117**.
- **`Tick`:** Matches `switch (m_Direction)` crush/texture fall-through in **125–215**; destroy via context callback (native `Destroy()`).
- **EV helpers** **506–621** mirror manual/tag paths; `CreateCeiling` invoked through `EvCeilingContext.Create` (chunk **796**), not reimplemented.
- No duplicate `CeilingActions` sim layer; VM is standalone testable port.
