# Chunk 887 — `RecalculateDrawnSubsectors`, `SerializeSubsectors`, and savegame portal/poly/reverb serializers (`p_saveg.cpp` 346–553).

## Scope

`RecalculateDrawnSubsectors`, `SerializeSubsectors`, and savegame portal/poly/reverb serializers (`p_saveg.cpp` 346–553).

## Native ranges

- `src/p_saveg.cpp` **FLevelLocals::RecalculateDrawnSubsectors** 346–359 (14 lines)
- `src/p_saveg.cpp` **FLevelLocals::SerializeSubsectors** 367–451 (85 lines)
- `src/p_saveg.cpp` **Serialize(FLinePortal)** 459–473 (15 lines)
- `src/p_saveg.cpp` **Serialize(FSectorPortal)** 481–497 (17 lines)
- `src/p_saveg.cpp` **Serialize(FPolyObj)** 505–531 (27 lines)
- `src/p_saveg.cpp` **Serialize(ReverbContainer)** 539–548 (10 lines)
- `src/p_saveg.cpp` **Serialize(zone_t)** 550–553 (4 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmPSavegSubsectorsPortals.cs`
Managed LOC (tally): 418.
Conversion commit: `56537f4f23716638ce9afd2b6c55507f785b30bb`.

## Padding review (2026-10-21)

**Behavior ported (yes/no)**

| Native | Behavior ported |
|--------|-----------------|
| `FLevelLocals::RecalculateDrawnSubsectors` **346–359** | **yes** |
| `FLevelLocals::SerializeSubsectors` **367–451** | **yes** |
| `Serialize(FLinePortal)` **459–473** | **yes** |
| `Serialize(FSectorPortal)` **481–497** | **yes** |
| `Serialize(FPolyObj)` **505–531** | **yes** |
| `Serialize(ReverbContainer*)` **539–548** | **yes** |
| `Serialize(zone_t)` **550–553** | **yes** |

| Managed | Native (`p_saveg.cpp`) |
|---------|----------------------|
| `RecalculateDrawnSubsectors` | **346–359** |
| `SerializeSubsectors` | **367–451** |
| `SerializeLinePortal` | **459–473** |
| `SerializeSectorPortal` | **481–497** |
| `SerializePolyObj` | **505–531** |
| `SerializeReverbContainer` | **539–548** |
| `SerializeZone` | **550–553** |

### Padding map (hull **346–553**)

| Native lines | Reason |
|--------------|--------|
| **360–366**, **452–458**, **474–480**, **498–504**, **532–538** | comment/separator gaps between credited functions (in band hull, no extra symbols) |

### Gross vs net

| | Value |
|--|------:|
| Gross native (`nativeLoc` sum of ranges) | **208** |
| Net behavior-ported | **208** |


**Post-review ratio:** 418/172 managed/native ≈ 243% (src file LOC at HEAD; see tally).

## Tests

- `VmPSavegSubsectorsPortalsTests.RecalculateDrawnSubsectors_SetsFlagWhenLinedefMapped`
- `VmPSavegSubsectorsPortalsTests.SerializeSubsectors_InvalidEncoding_RecalculatesFromMappedLines`
- `VmPSavegSubsectorsPortalsTests.SerializeSubsectors_WriteThenRead_RestoresDrawnFlags`
- `VmPSavegSubsectorsPortalsTests.SerializeLinePortal_Read_LoadsSeededFields`
- `VmPSavegSubsectorsPortalsTests.SerializePolyObj_OnRead_AppliesRotateAndMoveWhenOriginalPtsExist`
- `VmPSavegSubsectorsPortalsTests.SerializeReverbContainer_Read_ResolvesEnvironmentById`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 887 --to 887` (0 hard / 0 soft).

## Seams

`FSerializer` archive IO is mirrored by `SavegArchive` in the credited VM file; `S_FindEnvironment` maps to `FindEnvironment` + `EnvironmentCatalog` (native lookup table). `RotatePolyobj` / `MovePolyobj` are methods on `PolyObj` (native member calls).

## Credit rationale

Faithful whole-function ports for subsector drawn-bit save encoding and portal/poly/reverb save fields outside chunk10 ACS archive bands.
