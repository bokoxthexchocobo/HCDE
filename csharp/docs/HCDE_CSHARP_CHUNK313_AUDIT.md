# Chunk 313 — LoadDehSupp map sections (StateMap through ThingBits) + section errors

## Scope

Bundled `LoadDehSupp` table sections that were briefly split as tally rows 314–315; merged back into one conversion chunk (~124 native LOC). Covers actor/state mapping, sound and actor name lists, thing-bit metadata, unknown-section failure, and recoverable load errors. `RenderStyles` (3708–3724) is implemented in `VmDehackedDehSuppRenderStyles.cs` with tests but **not** credited here (already claimed in chunk 263).

## Native ranges

- `src/gamedata/d_dehacked.cpp` **StateMap** 3598–3652 (55 lines)
- `src/gamedata/d_dehacked.cpp` **SoundMap** 3653–3663 (11 lines)
- `src/gamedata/d_dehacked.cpp` **InfoNames** 3664–3679 (16 lines)
- `src/gamedata/d_dehacked.cpp` **ThingBits** 3680–3707 (28 lines)
- `src/gamedata/d_dehacked.cpp` **LoadDehSupp unknown section** 3781–3787 (7 lines)
- `src/gamedata/d_dehacked.cpp` **LoadDehSupp recoverable error** 3790–3796 (7 lines)

**Credited native LOC:** 124 (tally `nativeLoc` 124).

## Managed

`VmDehackedDehSuppStateMap.cs`, `VmDehackedDehSuppSoundInfo.cs`, `VmDehackedDehSuppThingBits.cs` / `VmDehackedDehSuppRenderStyles.cs`
Managed LOC (tally): 280.
Conversion commit: `40ede3e2aa1f0990885b9acfc68278aac13df185`.

## Tests

- `VmDehackedDehSuppStateMapTests.ShouldAppendToStateMap_OnlyWhenEmpty`
- `VmDehackedDehSuppStateMapTests.IsPickupActor_DetectsMfSpecial`
- `VmDehackedDehSuppStateMapTests.TryValidateActorType_RejectsNonActor`
- `VmDehackedDehSuppStateMapTests.TryParseStateMapSection_BuildsDehExtStates`
- `VmDehackedDehSuppStateMapTests.TryParseStateMapSection_SkipsAppendWhenExisting`
- `VmDehackedDehSuppStateMapTests.TryParseStateMapSection_RejectsInvalidStateSpan`
- `VmDehackedDehSuppSoundInfoTests.ParseSoundMapSection_ResolvesEachSound`
- `VmDehackedDehSuppSoundInfoTests.TryParseInfoNamesSection_AcceptsKnownActors`
- `VmDehackedDehSuppSoundInfoTests.TryParseInfoNamesSection_RejectsUnknownActor`
- `VmDehackedDehSuppThingBitsTests.TryParseEntry_ValidatesBitRange`
- `VmDehackedDehSuppThingBitsTests.TryParseEntry_TruncatesLongName`
- `VmDehackedDehSuppThingBitsTests.TryParseThingBitsSection_ParsesRows`
- `VmDehackedDehSuppLumpGateTests.FormatUnknownSectionError_MatchesNativeMessage`
- `VmDehackedDehSuppLumpGateTests.TryHandleRecoverableLoadError_ReturnsFalse`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 313 --to 313` (0 hard / 0 soft).

## Seams

Engine types (`PClass`, `FState`, `S_FindSound`, scanner lump I/O) are injected via callbacks or omitted where the managed layer only models parse/validation rules.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
