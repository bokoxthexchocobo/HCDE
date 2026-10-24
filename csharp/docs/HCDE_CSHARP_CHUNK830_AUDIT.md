# Chunk 830 — FraggleScript lump loader from `fragglescript/t_load.cpp` (60–311).

## Scope

FraggleScript lump loader from `fragglescript/t_load.cpp` (60–311).

## Native ranges

- `src/playsim/fragglescript/t_load.cpp` **FScriptLoader::ParseInfoCmd** 60–189 (130 lines)
- `src/playsim/fragglescript/t_load.cpp` **FScriptLoader::ParseInfo** 198–270 (73 lines)
- `src/playsim/fragglescript/t_load.cpp` **T_LoadScripts** 279–295 (17 lines)
- `src/playsim/fragglescript/t_load.cpp` **T_AddSpawnedThing** 304–311 (8 lines)

## Headline gross vs net

| Anchor | `convertedBehaviorNative` |
| --- | ---: |
| After chunk **829** tally (`b4823af0`) | **97,236** |
| After chunk **830** tally (`7aa92e8b`) | **97,464** |
| **Net** | **+228** (= row gross **228**) |

**Credited native LOC:** 228 (tally `nativeLoc` 228).

## Portfolio delta

**Gross CLI band:** **252** lines in the UNCredited-range probe; tally credits **228** whole-function native LOC only.
**24** uncovered lines in that band are inter-function gaps, comments, or non-function lines (not credited).

| | Native LOC |
|---|---:|
| **Gross** (CLI UNCredited band) | **252** |
| **Credited** (whole functions, this row) | **228** |
| **Overlap trim** | **0** |
| **Net headline Δ** | **+228** (see `convertedBehaviorNative` in tally at land commit) |

## Managed

`VmFScriptLoad.cs`
Managed LOC (tally): 264.
Conversion commit: `915409bfeb278231469c03ca9d69dd63a984001f`.

## Padding review (2026-10-21)

| Managed | Native (`t_load.cpp`) |
|---------|------------------------|
| `ParseInfoCmd` | **60–189** |
| `ParseInfo` | **198–270** |
| `LoadScripts` | **279–295** |
| `AddSpawnedThing` | **304–311** |

**Post-review ratio:** 264/228 managed/native ≈ 116% (src file LOC at HEAD; see tally).

## Tests

- `VmFScriptLoadTests.ParseInfo_LevelInfo_SetsLevelName`
- `VmFScriptLoadTests.ParseInfo_DrownZero_ClearsAirSupply`
- `VmFScriptLoadTests.ParseInfo_ScriptsSection_ReturnsTrueAndSetsThinker`
- `VmFScriptLoadTests.LoadScripts_SwapsTranslatorSlotsWhenFsExecute`
- `VmFScriptLoadTests.AddSpawnedThing_PushesWhenThinkerPresent`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 830 --to 830` (0 hard / 0 soft).

## Seams

Map lump IO, music/sky/console emulation, thinker factory, and translator table are injected on `LevelLoadContext`.

## Credit rationale

First credit for `FScriptLoader` parsing and `T_LoadScripts` / `T_AddSpawnedThing` entry points.
