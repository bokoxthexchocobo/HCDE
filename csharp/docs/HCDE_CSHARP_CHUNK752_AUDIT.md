# Chunk 752 — Model ID / bone override / anim archive plus `DActorModelData` and `DViewPosition` serialize (`p_mobj.cpp` 1749–1950).

## Scope

Model ID / bone override / anim archive plus `DActorModelData` and `DViewPosition` serialize (`p_mobj.cpp` 1749–1950).

## Native ranges

- `src/playsim/p_mobj.cpp` **SerializeModelID** 1749–1772 (24 lines)
- `src/playsim/p_mobj.cpp` **SerializeBoneOverrideComponent** 1775–1785 (11 lines)
- `src/playsim/p_mobj.cpp` **Serialize(BoneOverride)** 1788–1796 (9 lines)
- `src/playsim/p_mobj.cpp` **Serialize(TRS)** 1798–1806 (9 lines)
- `src/playsim/p_mobj.cpp` **Serialize(ModelOverride)** 1808–1815 (8 lines)
- `src/playsim/p_mobj.cpp` **Serialize(AnimModelOverride)** 1817–1830 (14 lines)
- `src/playsim/p_mobj.cpp` **Serialize(ModelAnim)** 1832–1845 (14 lines)
- `src/playsim/p_mobj.cpp` **Serialize(ModelAnimFrame)** 1847–1918 (72 lines)
- `src/playsim/p_mobj.cpp` **DActorModelData::Serialize** 1920–1934 (15 lines)
- `src/playsim/p_mobj.cpp` **DActorModelData::OnDestroy** 1936–1943 (8 lines)
- `src/playsim/p_mobj.cpp` **DViewPosition::Serialize** 1945–1950 (6 lines)

**Credited native LOC:** 190 (tally `nativeLoc` 190).

## Managed

`VmPMobjSerializeModel.cs`
Managed LOC (tally): 206.
Conversion commit: `b1d0ff0df710943f0142e0c130b6a35c07ddad32`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SerializeModelId` | `SerializeModelID` 1749–1772 |
| `SerializeBoneOverrideComponent` | `SerializeBoneOverrideComponent` 1775–1785 |
| `SerializeBoneOverride` | `Serialize(BoneOverride)` 1788–1796 |
| `SerializeTrs` | `Serialize(TRS)` 1798–1806 |
| `SerializeModelOverride` | `Serialize(ModelOverride)` 1808–1815 |
| `SerializeAnimModelOverride` | `Serialize(AnimModelOverride)` 1817–1830 |
| `SerializeModelAnim` | `Serialize(ModelAnim)` 1832–1845 |
| `ReadModelAnimFrameType` / `WriteModelAnimFrameType` | `Serialize(ModelAnimFrame)` 1847–1918 |
| `SerializeActorModelData` | `DActorModelData::Serialize` 1920–1934 |
| `ClearActorModelDataOnDestroy` | `DActorModelData::OnDestroy` 1936–1943 |
| `SerializeViewPosition` | `DViewPosition::Serialize` 1945–1950 |


**Post-review ratio:** 206/190 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSerializeModelTests.SerializeModelId_ReadMissingKey_ReturnsNegativeOne`
- `VmPMobjSerializeModelTests.SerializeModelId_WritePositiveId_EmitsPath`
- `VmPMobjSerializeModelTests.SerializeBoneOverrideComponent_WritesModeFields`
- `VmPMobjSerializeModelTests.SerializeBoneOverride_OpensObject`
- `VmPMobjSerializeModelTests.SerializeTrs_OpensObject`
- `VmPMobjSerializeModelTests.SerializeModelOverride_EmitsModelId`
- `VmPMobjSerializeModelTests.SerializeAnimModelOverride_ReadGuard`
- `VmPMobjSerializeModelTests.SerializeModelAnim_WritesFrameFields`
- `VmPMobjSerializeModelTests.ReadModelAnimFrameType_PrecalcMapsToNull`
- `VmPMobjSerializeModelTests.WriteModelAnimFrameType_Nullptr`
- `VmPMobjSerializeModelTests.SerializeActorModelData_WritesFieldKeys`
- `VmPMobjSerializeModelTests.ClearActorModelDataOnDestroy_ResetsCollections`
- `VmPMobjSerializeModelTests.SerializeViewPosition_WritesOffsetAndFlags`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 752 --to 752` (0 hard / 0 soft).

## Seams

`FSerializer`, `Models[]`, `FindModel`, and `Super::Serialize` IO are injected.

## Credit rationale

Re-ports retired chunk736 band with whole serialize helpers through view-position archive (same TU, uncredited lines).
