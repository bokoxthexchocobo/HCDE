# Chunk 752 — Whole model archive serialize helpers (`p_mobj.cpp` 1749–1918); excludes `DActorModelData` / `DViewPosition` class methods.

## Scope

Whole model archive serialize helpers (`p_mobj.cpp` 1749–1918); excludes `DActorModelData` / `DViewPosition` class methods.

## Native ranges

- `src/playsim/p_mobj.cpp` **SerializeModelID** 1749–1772 (24 lines)
- `src/playsim/p_mobj.cpp` **SerializeBoneOverrideComponent** 1774–1786 (13 lines)
- `src/playsim/p_mobj.cpp` **Serialize(BoneOverride)** 1788–1796 (9 lines)
- `src/playsim/p_mobj.cpp` **Serialize(TRS)** 1798–1806 (9 lines)
- `src/playsim/p_mobj.cpp` **Serialize(ModelOverride)** 1808–1815 (8 lines)
- `src/playsim/p_mobj.cpp` **Serialize(AnimModelOverride)** 1817–1830 (14 lines)
- `src/playsim/p_mobj.cpp` **Serialize(ModelAnim)** 1832–1845 (14 lines)
- `src/playsim/p_mobj.cpp` **Serialize(ModelAnimFrame)** 1847–1918 (72 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmPMobjSerializeModel.cs`
Managed LOC (tally): 316.
Conversion commit: `609d8ddb5368f7a1b65bc499424e3bba66dcf196`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-22):** Credits eight free functions **1749–1918** only. Excludes `DActorModelData::Serialize` **1920–1934**, `DActorModelData::OnDestroy` **1936–1943**, and `DViewPosition::Serialize` **1945–1950** (require `Super::Serialize` and collection types).

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SerializeModelId` | `SerializeModelID` 1749–1772 |
| `SerializeBoneOverrideComponent` | `SerializeBoneOverrideComponent` (template) 1774–1786 |
| `SerializeBoneOverride` | `Serialize(BoneOverride)` 1788–1796 |
| `SerializeTrs` / `IModelArchiveFieldSink.ArcVector3` | `Serialize(TRS)` 1798–1806 |
| `SerializeModelOverride` | `Serialize(ModelOverride)` 1808–1815 |
| `SerializeAnimModelOverride` | `Serialize(AnimModelOverride)` 1817–1830 |
| `SerializeModelAnim` | `Serialize(ModelAnim)` 1832–1845 |
| `ShouldClearLegacyInterp` | legacy read branch 1852–1855 |
| `ReadModelAnimFrameType` | read `type` dispatch 1858–1883 |
| `SerializeModelAnimFrame` | `Serialize(ModelAnimFrame)` 1847–1918 |
| `WriteModelAnimFrameTypeToken` | write `type` branch 1888–1914 |
| `ModelAnimFrameInterpFields` | `ModelAnimFrameInterp` read/write 1866–1870, 1897–1899 |


**Post-review ratio:** 316/163 managed/native ≈ 194% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSerializeModelTests.SerializeModelId_ReadMissingKey_ReturnsNegativeOne`
- `VmPMobjSerializeModelTests.SerializeModelId_WritePositiveId_EmitsPath`
- `VmPMobjSerializeModelTests.SerializeBoneOverrideComponent_RoundTripsFields`
- `VmPMobjSerializeModelTests.SerializeTrs_WritesThreeVectors`
- `VmPMobjSerializeModelTests.SerializeModelOverride_EmitsModelIdAndSkins`
- `VmPMobjSerializeModelTests.SerializeAnimModelOverride_ReadGuard`
- `VmPMobjSerializeModelTests.SerializeModelAnim_ReadsAllFields`
- `VmPMobjSerializeModelTests.SerializeModelAnimFrame_LegacyFirstFrameClearsInterp`
- `VmPMobjSerializeModelTests.SerializeModelAnimFrame_InterpRoundTrip`
- `VmPMobjSerializeModelTests.ReadModelAnimFrameType_PrecalcMapsToNull`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 752 --to 752` (0 hard / 0 soft).

## Seams

`FSerializer`, `Models[]`, `FindModel`, and nested `arc` IO are injected via `IModelArchiveFieldSink` in uncredited `ModelArchiveFieldSink.cs` / `ModelArchiveContext`.

## Credit rationale

Re-ports retired chunk736 band with faithful whole-function serialize helpers only (1920+ deferred until full class archive ports).
